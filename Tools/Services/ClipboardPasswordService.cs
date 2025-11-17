using NHotkey.Wpf;
using Tools.Library.Services;

namespace Tools.Services
{
    public interface IClipboardPasswordService
    {
        void RegisterHotKeys();
    }

    public class ClipboardPasswordService : IClipboardPasswordService
    {
        private readonly ISettingsService _settingsService;

        public ClipboardPasswordService(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public void RegisterHotKeys()
        {
            HotkeyManager.Current.AddOrReplace(
                 "ClipboardPasswordHotkey",
                 Key.V,
                 ModifierKeys.Control | ModifierKeys.Shift,
                 async (s, args) =>
                 {
                     string password = await GetDecryptedPasswordAsync();
                     if (!string.IsNullOrEmpty(password))
                         Clipboard.SetText(password);
                 });
        }

        private async Task<string> GetDecryptedPasswordAsync()
        {
            var settings = await _settingsService.GetSettingsAsync();
            var encrypted = settings.ClipboardPassword?.EncryptedPassword;
            if (string.IsNullOrEmpty(encrypted))
                return string.Empty;
            try
            {
                return Encoding.UTF8.GetString(Convert.FromBase64String(encrypted));
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}