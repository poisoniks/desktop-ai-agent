using System.Windows;

namespace DesktopAIAgent.Service
{
    public interface IGlobalHotkeyService
    {
        void Dispose();
        void Register(Window window, int id, uint modifiers, uint key);
        void Subscribe(EventHandler handler);
    }
}
