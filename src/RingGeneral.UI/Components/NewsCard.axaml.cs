using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using ReactiveUI;
using System;
using System.Reactive;

namespace RingGeneral.UI.Components;

/// <summary>
/// News/message card component for the Inbox system
/// Displays messages with type-specific icons, timestamps, and quick actions
/// Supports: Contract, Injury, Scout Report, Progress, Finance, Alert
/// </summary>
public partial class NewsCard : UserControl
{
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<NewsCard, string>(
            nameof(Title),
            defaultValue: string.Empty,
            defaultBindingMode: BindingMode.OneWay);

    public static readonly StyledProperty<string> MessageProperty =
        AvaloniaProperty.Register<NewsCard, string>(
            nameof(Message),
            defaultValue: string.Empty,
            defaultBindingMode: BindingMode.OneWay);

    public static readonly StyledProperty<DateTime> TimestampProperty =
        AvaloniaProperty.Register<NewsCard, DateTime>(
            nameof(Timestamp),
            defaultValue: DateTime.Now,
            defaultBindingMode: BindingMode.OneWay);

    public static readonly StyledProperty<string> TimestampTextProperty =
        AvaloniaProperty.Register<NewsCard, string>(
            nameof(TimestampText),
            defaultValue: string.Empty,
            defaultBindingMode: BindingMode.OneWay);

    public static readonly StyledProperty<MessageType> MessageTypeProperty =
        AvaloniaProperty.Register<NewsCard, MessageType>(
            nameof(MessageType),
            defaultValue: MessageType.Alert,
            defaultBindingMode: BindingMode.OneWay);

    public static readonly StyledProperty<bool> IsUnreadProperty =
        AvaloniaProperty.Register<NewsCard, bool>(
            nameof(IsUnread),
            defaultValue: false,
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<string> IconTextProperty =
        AvaloniaProperty.Register<NewsCard, string>(
            nameof(IconText),
            defaultValue: "ℹ",
            defaultBindingMode: BindingMode.OneWay);

    public static readonly StyledProperty<bool> IsContractTypeProperty =
        AvaloniaProperty.Register<NewsCard, bool>(
            nameof(IsContractType),
            defaultValue: false,
            defaultBindingMode: BindingMode.OneWay);

    public static readonly StyledProperty<bool> IsInjuryTypeProperty =
        AvaloniaProperty.Register<NewsCard, bool>(
            nameof(IsInjuryType),
            defaultValue: false,
            defaultBindingMode: BindingMode.OneWay);

    public static readonly StyledProperty<bool> IsScoutTypeProperty =
        AvaloniaProperty.Register<NewsCard, bool>(
            nameof(IsScoutType),
            defaultValue: false,
            defaultBindingMode: BindingMode.OneWay);

    public static readonly StyledProperty<bool> IsProgressTypeProperty =
        AvaloniaProperty.Register<NewsCard, bool>(
            nameof(IsProgressType),
            defaultValue: false,
            defaultBindingMode: BindingMode.OneWay);

    public static readonly StyledProperty<bool> IsFinanceTypeProperty =
        AvaloniaProperty.Register<NewsCard, bool>(
            nameof(IsFinanceType),
            defaultValue: false,
            defaultBindingMode: BindingMode.OneWay);

    public static readonly StyledProperty<bool> IsAlertTypeProperty =
        AvaloniaProperty.Register<NewsCard, bool>(
            nameof(IsAlertType),
            defaultValue: false,
            defaultBindingMode: BindingMode.OneWay);

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Message
    {
        get => GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public DateTime Timestamp
    {
        get => GetValue(TimestampProperty);
        set => SetValue(TimestampProperty, value);
    }

    public string TimestampText
    {
        get => GetValue(TimestampTextProperty);
        private set => SetValue(TimestampTextProperty, value);
    }

    public MessageType MessageType
    {
        get => GetValue(MessageTypeProperty);
        set => SetValue(MessageTypeProperty, value);
    }

    public bool IsUnread
    {
        get => GetValue(IsUnreadProperty);
        set => SetValue(IsUnreadProperty, value);
    }

    public string IconText
    {
        get => GetValue(IconTextProperty);
        private set => SetValue(IconTextProperty, value);
    }

    public bool IsContractType
    {
        get => GetValue(IsContractTypeProperty);
        private set => SetValue(IsContractTypeProperty, value);
    }

    public bool IsInjuryType
    {
        get => GetValue(IsInjuryTypeProperty);
        private set => SetValue(IsInjuryTypeProperty, value);
    }

    public bool IsScoutType
    {
        get => GetValue(IsScoutTypeProperty);
        private set => SetValue(IsScoutTypeProperty, value);
    }

    public bool IsProgressType
    {
        get => GetValue(IsProgressTypeProperty);
        private set => SetValue(IsProgressTypeProperty, value);
    }

    public bool IsFinanceType
    {
        get => GetValue(IsFinanceTypeProperty);
        private set => SetValue(IsFinanceTypeProperty, value);
    }

    public bool IsAlertType
    {
        get => GetValue(IsAlertTypeProperty);
        private set => SetValue(IsAlertTypeProperty, value);
    }

    public ReactiveCommand<Unit, Unit> MarkAsReadCommand { get; }
    public ReactiveCommand<Unit, Unit> ArchiveCommand { get; }
    public ReactiveCommand<Unit, Unit> DeleteCommand { get; }

    public NewsCard()
    {
        InitializeComponent();
        DataContext = this;

        MarkAsReadCommand = ReactiveCommand.Create(MarkAsRead);
        ArchiveCommand = ReactiveCommand.Create(Archive);
        DeleteCommand = ReactiveCommand.Create(Delete);

        // Update timestamp text when timestamp changes
        this.GetObservable(TimestampProperty).Subscribe(timestamp =>
        {
            TimestampText = GetRelativeTimestamp(timestamp);
        });

        // Update icon and type flags when message type changes
        this.GetObservable(MessageTypeProperty).Subscribe(type =>
        {
            UpdateMessageType(type);
        });
    }

    private void MarkAsRead()
    {
        IsUnread = false;
    }

    private void Archive()
    {
        // To be implemented by parent - raise event or call service
        System.Diagnostics.Debug.WriteLine($"[NewsCard] Archive: {Title}");
    }

    private void Delete()
    {
        // To be implemented by parent - raise event or call service
        System.Diagnostics.Debug.WriteLine($"[NewsCard] Delete: {Title}");
    }

    private void UpdateMessageType(MessageType type)
    {
        IsContractType = type == MessageType.Contract;
        IsInjuryType = type == MessageType.Injury;
        IsScoutType = type == MessageType.ScoutReport;
        IsProgressType = type == MessageType.Progress;
        IsFinanceType = type == MessageType.Finance;
        IsAlertType = type == MessageType.Alert;

        IconText = type switch
        {
            MessageType.Contract => "📝",
            MessageType.Injury => "🏥",
            MessageType.ScoutReport => "🔍",
            MessageType.Progress => "📈",
            MessageType.Finance => "💰",
            MessageType.Alert => "⚠",
            _ => "ℹ"
        };
    }

    private static string GetRelativeTimestamp(DateTime timestamp)
    {
        var now = DateTime.Now;
        var diff = now - timestamp;

        if (diff.TotalMinutes < 1)
            return "À l'instant";
        if (diff.TotalMinutes < 60)
            return $"Il y a {(int)diff.TotalMinutes} min";
        if (diff.TotalHours < 24)
            return $"Il y a {(int)diff.TotalHours}h";
        if (diff.TotalDays < 7)
            return $"Il y a {(int)diff.TotalDays}j";
        if (diff.TotalDays < 30)
            return $"Il y a {(int)(diff.TotalDays / 7)} semaines";
        if (diff.TotalDays < 365)
            return $"Il y a {(int)(diff.TotalDays / 30)} mois";

        return timestamp.ToString("dd MMM yyyy");
    }
}

public enum MessageType
{
    Contract,
    Injury,
    ScoutReport,
    Progress,
    Finance,
    Alert
}
