using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using DesktopAIAgent.Util;

namespace DesktopAIAgent.ViewModel
{
    public class ChatMessage
    {
        public string Text { get; set; } = "";
        public bool IsUser { get; set; }
    }

    public class ChatViewModel : INotifyPropertyChanged
    {
        private readonly object _lock = new object();
        public ObservableCollection<ChatMessage> Messages { get; } = new();
        public string SessionId { get; } = Guid.NewGuid().ToString();

        private string _messageText = "";
        public string MessageText
        {
            get => _messageText;
            set { _messageText = value; OnPropertyChanged(nameof(MessageText)); }
        }

        public ICommand SendMessageCommand { get; }

        public ChatViewModel()
        {
            BindingOperations.EnableCollectionSynchronization(Messages, _lock);

            SendMessageCommand = new RelayCommand(
                execute: async (o) => await SendMessage(),
                canExecute: (o) => !string.IsNullOrWhiteSpace(MessageText)
            );
        }

        private async Task SendMessage()
        {
            var text = MessageText;
            MessageText = "";

            ReceiveMessage(text, true);

            await Task.Delay(500);
            ReceiveMessage($"Echo: {text}", false);
        }

        public void ReceiveMessage(string text, bool isUser)
        {
            lock (_lock)
            {
                Messages.Add(new ChatMessage { Text = text, IsUser = isUser });
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
