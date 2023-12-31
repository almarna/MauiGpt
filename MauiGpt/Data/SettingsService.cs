﻿using MauiGpt.Dto;
using Microsoft.Extensions.Configuration;
using System;
using System.Net;
using System.Reflection;
using System.Text.Json;

namespace MauiGpt.Data;

public class SettingsService
{
    private readonly string _filePath;

    private SettingsDto _settingsDto = new SettingsDto { CurrentModel = -1, Models = new List<ModelsDto>() };

    public SettingsService()
    {
        string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        _filePath = Path.Combine(folderPath, "models.json");

        Load();

    }

    public ModelsDto GetCurrent()
    {
        return _settingsDto.GetCurrentModel();
    }

    public IEnumerable<ModelsDto> GetAll()
    {
        return _settingsDto.Models.Select(model => model.Clone());
    }

    public int SaveModel(ModelsDto newModel)
    {
        if (newModel.Id < 0)
        {
            newModel.Id = _settingsDto.NextId();
        }

        _settingsDto.InsertOrUpdateModel(newModel);

        WriteSettingsToFile();

        return newModel.Id;
    }

    private void WriteSettingsToFile()
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        var jsonString = JsonSerializer.Serialize(_settingsDto, options);

        File.WriteAllText(_filePath, jsonString);
    }

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
        _settingsDto.RemoveModel(id);
        WriteSettingsToFile();
    }

    public void SetCurrentModel(int newModelId)
    {
        if (_settingsDto.SetCurrentModel(newModelId))
        {
            WriteSettingsToFile();
        }
    }
}