using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace LocalSettings
{
    public class LocalSettingService : ILocalSettingService
    {
        private readonly object _sync = new object();

        private Dictionary<string, string> _settings = new Dictionary<string, string>();

        public LocalSettingService()
        {
        }

        public FileInfo? SettingFile { get; private set; }

        public SettingWriteMode WriteMode { get; private set; } = SettingWriteMode.OnChange;

        public bool IsInitialized { get; private set; }

        public void Initialize(
            FileInfo settingFile,
            SettingWriteMode writeMode)
        {
            lock (_sync)
            {
                WriteMode = writeMode;
                SettingFile = settingFile;
                var folder = settingFile.Directory;
                if (!(folder?.Exists ?? false))
                {
                    folder?.Create();
                }

                ReadSettings();

                IsInitialized = true;
            }
        }

        public int GetInt(string key)
        {
            var found = Get(key);
            if (found == null)
            {
                return 0;
            }

            return int.TryParse(found, NumberStyles.Any, CultureInfo.InvariantCulture, out var intValue) ? intValue : 0;
        }

        public decimal GetDecimal(string key)
        {
            var found = Get(key);
            if (found == null)
            {
                return 0;
            }

            return decimal.TryParse(found, NumberStyles.Any, CultureInfo.InvariantCulture, out var intValue) ? intValue : 0m;
        }

        public DateTime GetDateTime(string key)
        {
            var found = Get(key);
            if (found == null)
            {
                return DateTime.MinValue;
            }

            return DateTime.TryParse(found, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var intValue) ? intValue : DateTime.MinValue;
        }

        public string? Get(string key)
        {
            VerifyInitialized();
            lock (_sync)
            {
                key = key?.ToLowerInvariant() ?? "";
                return _settings.ContainsKey(key) ? _settings[key] : null;
            }
        }

        public void Set(string key, int value) => Set(key, value.ToString(CultureInfo.InvariantCulture));

        public void Set(string key, decimal value) => Set(key, value.ToString("N", CultureInfo.InvariantCulture));

        public void Set(string key, DateTime value) => Set(key, value.ToString("o", CultureInfo.InvariantCulture));

        public void Set(string key, string value)
        {
            VerifyInitialized();
            lock (_sync)
            {
                key = key?.ToLowerInvariant() ?? "";
                if (string.IsNullOrEmpty(key))
                {
                    return;
                }

                if (!_settings.ContainsKey(key))
                {
                    _settings.Add(key, value);
                }
                else
                {
                    _settings[key] = value;
                }

                if (WriteMode == SettingWriteMode.OnChange)
                {
                    WriteSettings();
                }
            }
        }

        public void WriteSettings()
        {
            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            var yaml = serializer.Serialize(_settings);
            File.WriteAllText(SettingFile!.FullName, yaml);
        }

        private void VerifyInitialized()
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException("The setting service has not been initialized. Call the Initialize() method first.");
            }
        }

        private void ReadSettings()
        {
            if (!(SettingFile?.Exists ?? false))
            {
                return;
            }

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            var content = File.ReadAllText(SettingFile.FullName);
            _settings = deserializer.Deserialize<Dictionary<string, string>>(content);
        }
    }
}