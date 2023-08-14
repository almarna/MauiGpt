namespace MauiGpt.Dto;

public class SettingsDto
{
    public int CurrentModel { get; set; }
    public IList<ModelsDto> Models { get; set; } = new List<ModelsDto>();
}