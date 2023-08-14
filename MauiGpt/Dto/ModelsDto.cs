namespace MauiGpt.Dto;

public class ModelsDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Endpoint { get; set; }
    public string AuthKey { get; set; }
    public string Model { get; set; }

    public ModelsDto Clone()
    {
        return (ModelsDto)MemberwiseClone();
    }

    public bool IsEquivalent(ModelsDto otherModel)
    {
        if (otherModel == null)
        {
            return false;
        }

        return otherModel.Endpoint == Endpoint && otherModel.AuthKey == AuthKey && otherModel.Model == Model;
    }
}