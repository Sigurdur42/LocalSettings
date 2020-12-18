using System.IO;

namespace LocalSettings
{
    public interface ILocalSettingService
    {
        bool IsInitialized { get; }

        void Initialize(
            FileInfo settingFile,
            SettingWriteMode writeMode);

        string? Get(string key);
        int GetInt(string key);

        void Set(string key, string value);
        void Set(string key, int value);

        void WriteSettings();
    }
}