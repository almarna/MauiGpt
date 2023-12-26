using MauiGpt.Data.DbInfo;
using MauiGpt.Dto;
using MauiGpt.Interfaces;

namespace MauiGpt.Data
{
    public class ChatServiceFactory
    {
        private readonly SettingsService _settingsService;

        private ModelsDto _cachedModel;
        private IChatService _chatService;

        public ChatServiceFactory(SettingsService settingsService)
        {
            _settingsService = settingsService;
        }
        public IChatService GetChatService()
        {
            var modelsDto = _settingsService.GetCurrent();
            if ((_cachedModel?.IsEquivalent(modelsDto) ?? false) == false)
            {
                _cachedModel = modelsDto.Clone();
                _chatService = modelsDto.UseSemanticKernel
                    ? new AiDbService(_cachedModel)
                    : new OpenAiService(_cachedModel);
            }

            return _chatService;
        }
    }
}
