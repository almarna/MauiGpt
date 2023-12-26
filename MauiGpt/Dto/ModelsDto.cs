namespace MauiGpt.Dto;

public class ModelsDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Endpoint { get; set; }
    public string AuthKey { get; set; }
    public string Model { get; set; }
    public bool UseSemanticKernel { get; set; }
    public Dictionary<string, string> ConnectionStrings { get; set; } = new Dictionary<string, string>();

    public ModelsDto Clone()
    {
        var clone = (ModelsDto)MemberwiseClone();
        clone.ConnectionStrings = new Dictionary<string, string>(this.ConnectionStrings);

        return clone;
    }

    public bool IsEquivalent(ModelsDto otherModel)
    {
        if (otherModel == null)
        {
            return false;
        }

        var localConnStr = ConnectionStrings ?? new Dictionary<string, string>();
        var otherConnStr = otherModel.ConnectionStrings ?? new Dictionary<string, string>();


        if (localConnStr.Count != otherConnStr.Count)
        {
            return false;
        }

        bool allConnectionStringsEqual = localConnStr.OrderBy(kvp => kvp.Key).SequenceEqual(otherConnStr.OrderBy(kvp => kvp.Key));

        return allConnectionStringsEqual && otherModel.Endpoint == Endpoint && otherModel.AuthKey == AuthKey && otherModel.Model == Model;
    }
}