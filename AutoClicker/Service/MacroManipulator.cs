using System;
using System.IO;
using System.Xml.Linq;
using System.Windows.Forms;
using System.Diagnostics;
using System.Linq;

namespace AutoClicker.Service
{
    public class MacroManipulator
    {
        // Costanti per le azioni
        private const string ACTION_CODE_INFURIARSI = "13";
        private const string ACTION_SUBCODE_INFURIARSI = "28";
        private const string ACTION_SUBMENUTYPE_INFURIARSI = "1";

        private const string ACTION_CODE_SACCARACCOLTA = "1";
        private const string ACTION_SUBCODE_SACCARACCOLTA = "0";
        private const string ACTION_SUBMENUTYPE_SACCARACCOLTA = "2";
        private const string ACTION_TEXT_SACCARACCOLTA = "'saccaraccolta";

        // Costanti per i tasti
        private const string KEY_O = "111"; // Codice ASCII per 'O'
        private const string KEY_I = "105"; // Codice ASCII per 'I'
        private const string KEY_NUMPAD2 = "1073741918"; // Codice SDL per NumPad 2 (Down)

        // Risultato dell'elaborazione
        public class MacroResult
        {
            public bool HasInfuriarsi { get; set; }
            public string InfuriarsiKeyInfo { get; set; }
            public bool HasSaccaraccolta { get; set; }
            public string SaccaraccoltaKeyInfo { get; set; }
            public bool NeedsClientRestart { get; set; }
            public bool HaveBaseFuria { get; set; } // Nuova proprietà per verificare se esiste già una macro furia di base
        }

        public static MacroResult ManipulateMacros(string xmlFilePath, string outputFilePath = null, bool haveInfuriarsi = false)
        {
            // Se non viene specificato un percorso di output, sovrascrive il file di input
            if (string.IsNullOrEmpty(outputFilePath))
                outputFilePath = xmlFilePath;

            MacroResult result = new MacroResult();
            result.NeedsClientRestart = false;

            try
            {
                // Carica il documento XML
                XDocument xmlDoc = XDocument.Load(xmlFilePath);
                XElement macrosElement = xmlDoc.Root;
                bool fileModified = false;

                // Verifica se esiste una qualsiasi macro con azione "Infuriarsi", indipendentemente dalla combinazione di tasti
                var anyBaseFuriaMacro = FindAnyMacroWithAction(macrosElement,
                    ACTION_CODE_INFURIARSI, ACTION_SUBCODE_INFURIARSI, ACTION_SUBMENUTYPE_INFURIARSI);

                result.HaveBaseFuria = anyBaseFuriaMacro != null;

                if (result.HaveBaseFuria)
                {
                    Console.WriteLine("Trovata una macro di base per 'Infuriarsi'");
                }

                // 1. Cerca macro con azione "Infuriarsi" e combinazione Shift+O
                var infuriarsiMacroShiftO = FindMacroWithActionAndKeys(macrosElement,
                    ACTION_CODE_INFURIARSI, ACTION_SUBCODE_INFURIARSI, ACTION_SUBMENUTYPE_INFURIARSI,
                    KEY_O, false, true, false);  // ctrl=false, shift=true, alt=false

                if (infuriarsiMacroShiftO == null)
                {
                    // Non esiste una macro con questa azione e questa combinazione di tasti, la creiamo
                    CreateMacroWithHotkey(macrosElement, "infuriarsi_shift_o_uomrt", KEY_O, false, false, true,
                        ACTION_CODE_INFURIARSI, ACTION_SUBCODE_INFURIARSI, ACTION_SUBMENUTYPE_INFURIARSI);

                    result.HasInfuriarsi = true;
                    result.InfuriarsiKeyInfo = "Key: O, Combinazione: Shift+O";
                    Console.WriteLine("Aggiunta nuova macro per Shift+O con azione 'Infuriarsi'");
                    fileModified = true;
                    result.NeedsClientRestart = true;
                }
                else
                {
                    // Esiste già, registriamo l'informazione
                    result.HasInfuriarsi = true;
                    result.InfuriarsiKeyInfo = "Key: O, Combinazione: Shift+O";
                    Console.WriteLine("Macro con azione 'Infuriarsi' e combinazione Shift+O già presente");
                }

                // 2. Cerca macro con azione "saccaraccolta" e combinazione Shift+I
                var saccaraccoltaShiftI = FindMacroWithActionTextAndKeys(macrosElement,
                    ACTION_CODE_SACCARACCOLTA, ACTION_SUBCODE_SACCARACCOLTA, ACTION_SUBMENUTYPE_SACCARACCOLTA,
                    ACTION_TEXT_SACCARACCOLTA, KEY_I, false, true, false);  // ctrl=false, shift=true, alt=false

                if (saccaraccoltaShiftI == null)
                {
                    // Non esiste una macro con questa azione e questa combinazione di tasti, la creiamo
                    CreateMacroWithHotkey(macrosElement, "saccaraccolta_shift_i_uomrt", KEY_I, false, false, true,
                        ACTION_CODE_SACCARACCOLTA, ACTION_SUBCODE_SACCARACCOLTA,
                        ACTION_SUBMENUTYPE_SACCARACCOLTA, ACTION_TEXT_SACCARACCOLTA);

                    result.HasSaccaraccolta = true;
                    result.SaccaraccoltaKeyInfo = "Key: I, Combinazione: Shift+I";
                    Console.WriteLine("Aggiunta nuova macro per Shift+I con azione 'saccaraccolta'");
                    fileModified = true;
                    result.NeedsClientRestart = true;
                }
                else
                {
                    // Esiste già, registriamo l'informazione
                    result.HasSaccaraccolta = true;
                    result.SaccaraccoltaKeyInfo = "Key: I, Combinazione: Shift+I";
                    Console.WriteLine("Macro con azione 'saccaraccolta' e combinazione Shift+I già presente");
                }

                // Salva il documento modificato solo se sono state effettuate modifiche
                if (fileModified)
                {
                    xmlDoc.Save(outputFilePath);
                    Console.WriteLine($"File XML salvato come: {outputFilePath}");
                }
                else
                {
                    Console.WriteLine("Nessuna modifica apportata al file XML.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore durante la manipolazione del file XML: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// Cerca una qualsiasi macro che contiene l'azione specificata, indipendentemente dalla combinazione di tasti
        /// </summary>
        private static XElement FindAnyMacroWithAction(XElement macrosElement,
            string code, string subcode, string submenutype)
        {
            return macrosElement.Elements("macro")
                .FirstOrDefault(m =>
                    m.Element("actions")
                        ?.Elements("action")
                        .Any(a => a.Attribute("code")?.Value == code &&
                                 a.Attribute("subcode")?.Value == subcode &&
                                 a.Attribute("submenutype")?.Value == submenutype) == true);
        }

        

        // Trova macro con azione specifica e combinazione di tasti
        private static XElement FindMacroWithActionAndKeys(XElement macrosElement,
            string code, string subcode, string submenutype,
            string keyCode, bool ctrl, bool shift, bool alt)
        {
            return macrosElement.Elements("macro")
                .FirstOrDefault(m =>
                    m.Attribute("key")?.Value == keyCode &&
                    Convert.ToBoolean(m.Attribute("ctrl")?.Value) == ctrl &&
                    Convert.ToBoolean(m.Attribute("shift")?.Value) == shift &&
                    Convert.ToBoolean(m.Attribute("alt")?.Value) == alt &&
                    m.Element("actions")
                        ?.Elements("action")
                        .Any(a => a.Attribute("code")?.Value == code &&
                                 a.Attribute("subcode")?.Value == subcode &&
                                 a.Attribute("submenutype")?.Value == submenutype) == true);
        }

        // Trova macro con azione specifica, testo e combinazione di tasti
        private static XElement FindMacroWithActionTextAndKeys(XElement macrosElement,
            string code, string subcode, string submenutype, string text,
            string keyCode, bool ctrl, bool shift, bool alt)
        {
            return macrosElement.Elements("macro")
                .FirstOrDefault(m =>
                    m.Attribute("key")?.Value == keyCode &&
                    Convert.ToBoolean(m.Attribute("ctrl")?.Value) == ctrl &&
                    Convert.ToBoolean(m.Attribute("shift")?.Value) == shift &&
                    Convert.ToBoolean(m.Attribute("alt")?.Value) == alt &&
                    m.Element("actions")
                        ?.Elements("action")
                        .Any(a => a.Attribute("code")?.Value == code &&
                                 a.Attribute("subcode")?.Value == subcode &&
                                 a.Attribute("submenutype")?.Value == submenutype &&
                                 a.Attribute("text")?.Value == text) == true);
        }

        // Crea una nuova macro con combinazione di tasti e azione
        private static void CreateMacroWithHotkey(
            XElement macrosElement, string macroName, string keyCode,
            bool ctrl, bool alt, bool shift,
            string actionCode, string actionSubcode, string actionSubmenutype,
            string actionText = null)
        {
            XElement actionElement = new XElement("action",
                new XAttribute("code", actionCode),
                new XAttribute("subcode", actionSubcode),
                new XAttribute("submenutype", actionSubmenutype)
            );

            // Aggiungi l'attributo testo solo se specificato
            if (!string.IsNullOrEmpty(actionText))
            {
                actionElement.Add(new XAttribute("text", actionText));
            }

            XElement newMacro = new XElement("macro",
                new XAttribute("name", macroName),
                new XAttribute("key", keyCode),
                new XAttribute("mousebutton", "0"),
                new XAttribute("wheelscroll", "False"),
                new XAttribute("wheelup", "False"),
                new XAttribute("alt", alt.ToString()),
                new XAttribute("ctrl", ctrl.ToString()),
                new XAttribute("shift", shift.ToString()),
                new XAttribute("closable", "True"),
                new XAttribute("movable", "True"),
                new XAttribute("icon", "False"),
                new XAttribute("icongump", "2240"),
                new XAttribute("icongumphue", "0"),
                new XAttribute("icontext", ""),
                new XElement("actions", actionElement)
            );

            // Aggiungi la nuova macro al documento
            macrosElement.Add(newMacro);
        }
    }
}