namespace MauiGpt.Dto;

public class SettingsDto
{
    public int CurrentModel { get; set; }
    public IList<ModelsDto> Models { get; set; } = new List<ModelsDto>();

    public void InsertOrUpdateModel(ModelsDto model)
    {
        RemoveModel(model.Id);

        Models.Add(model);
    }

    public bool RemoveModel(int modelId)
    {
        var oldModel = Models.SingleOrDefault(x => x.Id == modelId);
        if (oldModel != null)
        {
            Models.Remove(oldModel);
            return true;
        }

        return false;
    }

    public int NextId()
    {
        return Models.Any() ? Models.Max(model => model.Id) + 1 : 0;
    }

    public bool SetCurrentModel(int newModelId)
    {
        if (Models.Any(item => item.Id == newModelId))
        {
            CurrentModel = newModelId;
            return true;
        }

        return false;
    }

    public ModelsDto GetCurrentModel()
    {
        var current = Models.SingleOrDefault(dto => dto.Id == CurrentModel) ?? Models.FirstOrDefault();
        CurrentModel = current?.Id ?? -1;
        return current?.Clone();
    }
}