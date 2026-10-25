using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Storage;

namespace ATHENA.AthenaAI
{
    internal class IASettings
    {
        public string Model { get; set; } = string.Empty;
        public string SystemPrompt { get; set; } = string.Empty;

        public void Salvar()
        { 
            Preferences.Set("AI_Model", Model);

            Preferences.Set("AI_SystemPrompt", SystemPrompt);
        }
        public void Carregar()
        {
            Model = Preferences.Get("AI_Model", "gemini-3.5-flash-lite");
            SystemPrompt = Preferences.Get("AI_SystemPrompt", "Responda de forma clara e objetiva");
        }
    }
}
