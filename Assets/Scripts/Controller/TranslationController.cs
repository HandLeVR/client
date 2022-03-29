using System;
using System.IO;
using UnityEngine;

namespace translator
{
    
    /// <summary>
    /// Class for the different translations. Needs to be expanded when more languages are added.
    /// </summary>
    public class Translation
    {
        public string id { get; set; }
        public string de_DE { get; set; }
        public string en_US { get; set; }
    }
    
    /// <summary>
    /// Allows to find the correct translation for a string by using a translation file
    /// </summary>
    public class TranslationController : Singleton<TranslationController>
    {
        private Translation[] translations;
        private const char lineSeparator = '\n';
        private const char fieldSeparator = ';';

        // Prevent non-singleton constructor use.
        protected TranslationController(){}

        public void Start()
        {
            Init();
        }
        
        /// <summary>
        /// Initialize the translation if needed.
        /// </summary>
        private void Init()
        {
            if (translations != null)
                return;
            
            // Splits the CSV file by line
            string[] records = File.ReadAllText(DataController.Instance.translationFilePath).Split(lineSeparator);
            translations = new Translation[records.Length];
            int pos = 0;
            // Reads the CSV file into the translations array
            foreach (string record in records)
            {
                Translation tempTranslation = new Translation();
                int i = 1;
                // Splits the CSV line by field
                string[] fields = record.Split(fieldSeparator);
                foreach (string field in fields)
                {
                    if (i == 1){tempTranslation.id = field;}
                    if (i == 2){tempTranslation.de_DE = field;}
                    if (i == 3){tempTranslation.en_US = field;}
                    i++;
                }
                translations[pos] = tempTranslation;
                pos++;
            }
        }
        
        /// <summary>
        /// Gets the translation determined by the id and uses german as the default language.
        /// </summary>
        public string Translate(string id, params object[] formatStrings)
        {
            return Translate(id, id, formatStrings);
        }
        
        public string TranslateWithDefault(string id, string defaultString, params object[] formatStrings)
        {
            return Translate(id, defaultString, formatStrings);
        }

        // This function is called with the id of a part (its file name) and
        // the language that is set by the application to provide the 
        // stored translation.
        private string Translate(string id, string defaultString, params object[] formatStrings)
        {
            Init();
            string language = ConfigController.Instance.GetLanguage();
            string output = defaultString;
            foreach (var part in translations)
            {
                if (part.id == id)
                {
                    if (language == "en_US") 
                        output = part.en_US;
                    if (String.IsNullOrWhiteSpace(output) || language == "de_DE") 
                        output = part.de_DE;
                }
            }

            if (formatStrings.Length > 0)
                output = string.Format(output, TranslateArgs(formatStrings));
            return output;
        }

        private object[] TranslateArgs(params object[] formatStrings)
        {
            object[] result = new object[formatStrings.Length];
            for (int i = 0; i < formatStrings.Length; i++)
                result[i] = Translate(formatStrings[i].ToString());
            return result;
        }

        /// <summary>
        /// Gets a translation by adding the prefix "sub-task-type-" to the key.
        /// </summary>
        public string TranslateSubTaskType(string type, bool description = false)
        {
            return GetPrefixTranslation("sub-task-type-", type, description);
        }

        /// <summary>
        /// Gets a translation by adding the prefix "support-info-type-" to the key.
        /// </summary>
        public string TranslateSupportInfoType(string type, bool description = false)
        {
            return GetPrefixTranslation("support-info-type-", type, description);
        }

        /// <summary>
        /// Gets a translation by adding a prefix to the key.
        /// </summary>
        private string GetPrefixTranslation(string prefix, string type, bool description)
        {
            string key = string.Concat(prefix, type.ToLower().Replace(" ", "-"));
            if (description)
                return Translate(key + "-description");
            return Translate(key);
            
        }
    }
}

