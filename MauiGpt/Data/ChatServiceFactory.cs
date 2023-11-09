using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LabSemanticKernel.Lab3;
using MauiGpt.Dto;
using MauiGpt.Interfaces;

namespace MauiGpt.Data
{
    public class ChatServiceFactory
    {
        private readonly SettingsService _settingsService;

        public ChatServiceFactory(SettingsService settingsService)
        {
            _settingsService = settingsService;
        }
        public IChatService GetChatService(ChatServiceType type)
        {
            return type switch
            {
                ChatServiceType.Db => new AiDbService(_settingsService),
                ChatServiceType.Gpt => new OpenAiService(_settingsService),
                _ => new OpenAiService(_settingsService)
            };
        }
    }
}
