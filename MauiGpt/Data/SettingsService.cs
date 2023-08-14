using MauiGpt.Dto;
using Microsoft.Extensions.Configuration;
using System;
using System.Net;
using System.Reflection;
using System.Text.Json;

namespace MauiGpt.Data;

public class SettingsService
{
    private readonly string _filePath = Path.Combine(AppContext.BaseDirectory, "models.json");

    private SettingsDto _settingsDto = new SettingsDto { CurrentModel = -1, Models = new List<ModelsDto>() };

    public SettingsService()
    {
        //_config = new ConfigurationBuilder()
        //    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        //    .Build();
        //GetSettingsDto();
        //var modelsSection = _config.GetSection("Models");
        //var models = modelsSection.Get<SettingsDto[]>();
//        Save();
        Load();
    }

    public ModelsDto GetCurrent()
    {
        var current = _settingsDto.Models.SingleOrDefault(dto => dto.Id == _settingsDto.CurrentModel);
        return current?.Clone();
    }

    //public int CurrentModel()
    //{
    //    return _settingsDto.CurrentModel;
    //}

    public IEnumerable<ModelsDto> GetAll()
    {
        return _settingsDto.Models.Select(model => model.Clone());
    }


    //public ModelsDto GetById(int id)
    //{
    //    return _settingsDtos.SingleOrDefault(dto => dto.Id == id);
    //}

    //public IEnumerable<string> GetNames()
    //{
    //    return _settingsDtos.Select(dto => dto.Name);
    //}


    //public ModelsDto GetSettingsDto()
    //{
    //    string endpoint = _config.GetValue<string>("Endpoint");
    //    string authKey = _config.GetValue<string>("AuthKey");
    //    string model = _config.GetValue<string>("Model");

    //    return new ModelsDto { AuthKey = authKey, Endpoint = endpoint, Model = model };
    //}


    public int SaveModel(ModelsDto newModel)
    {
        int id;
        var oldModel = _settingsDto.Models.SingleOrDefault(model => model.Id == newModel.Id);
        if (oldModel == null)
        {
            int lastId = _settingsDto.Models.Max(model => model.Id);
            id = lastId + 1;
            var modelToAdd = newModel.Clone();
            modelToAdd.Id = id;
            _settingsDto.Models.Add(modelToAdd);
        }
        else
        {
            oldModel.Name = newModel.Name;
            oldModel.Endpoint = newModel.Endpoint;
            oldModel.AuthKey = newModel.AuthKey;
            oldModel.Model = newModel.Model;
            id = oldModel.Id;
        }

        WriteSettingsToFile();

        return id;
    }

    private void WriteSettingsToFile()
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        var jsonString = JsonSerializer.Serialize(_settingsDto, options);

        // Write the JSON string to a file
        File.WriteAllText(_filePath, jsonString);
    }

    //public void Save()
    //{
    //    var models = new ModelsDto[]
    //    {
    //        new ModelsDto { Id = 0, Name = "GPT 3 Turbo", AuthKey = "boo", Endpoint = "https://gemensam-openai.openai.azure.com/", Model = "gpt3" },
    //        new ModelsDto { Id = 1, Name = "Gpt4", AuthKey = "ak1", Endpoint = "ep1", Model = "m1" },
    //    };

    //    var settings = new SettingsDto { CurrentModel = 0, Models = models };

    //    var options = new JsonSerializerOptions { WriteIndented = true };
    //    var jsonString = JsonSerializer.Serialize(settings, options);

    //    // Write the JSON string to a file
    //    File.WriteAllText(_filePath, jsonString);
    //}

    public void Load()
    {
        try
        {
            string json = File.ReadAllText(_filePath);
            _settingsDto = JsonSerializer.Deserialize<SettingsDto>(json);
        }
        catch
        {
            _settingsDto = new SettingsDto { CurrentModel = -1, Models = new List<ModelsDto>() };
        }
    }

    public void DeleteModel(int id)
    {
        _settingsDto.Models = _settingsDto.Models.Where(model => model.Id != id).ToList();
        WriteSettingsToFile();
    }

    public void SetModel(int newModelId)
    {
        if (_settingsDto.Models.Any(item => item.Id == newModelId))
        {
            _settingsDto.CurrentModel = newModelId;
            WriteSettingsToFile();
        }
    }
}