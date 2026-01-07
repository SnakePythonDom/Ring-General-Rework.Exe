# Mode Plein Écran - Ring General

## Configuration Actuelle

L'application s'ouvre désormais en **mode maximisé** par défaut.

```xml
WindowState="Maximized"
```

## Activation du Mode Borderless (Sans Bordures)

Pour un mode plein écran **sans bordures** (style immersif) :

### Méthode 1 : Modification du XAML

Éditez `/src/RingGeneral.UI/Views/Shell/MainWindow.axaml` :

```xml
<Window ...
        WindowState="FullScreen"
        SystemDecorations="None">
```

**OU** pour un mode borderless mais avec la barre de tâches visible :

```xml
<Window ...
        WindowState="Maximized"
        ExtendClientAreaToDecorationsHint="True"
        ExtendClientAreaTitleBarHeightHint="-1"
        ExtendClientAreaChromeHints="NoChrome">
```

### Méthode 2 : Toggle dynamique (recommandé)

Ajoutez un bouton F11 pour basculer entre les modes :

1. Dans `ShellViewModel.cs`, ajoutez :

```csharp
private bool _isFullScreen = false;

public ICommand ToggleFullScreenCommand { get; }

private void ToggleFullScreen()
{
    _isFullScreen = !_isFullScreen;
    // Signal pour changer l'état de la fenêtre
    MessageBus.Current.SendMessage(new FullScreenToggleMessage(_isFullScreen));
}
```

2. Dans `MainWindow.axaml.cs`, écoutez le message :

```csharp
MessageBus.Current.Listen<FullScreenToggleMessage>()
    .Subscribe(msg =>
    {
        WindowState = msg.IsFullScreen ? WindowState.FullScreen : WindowState.Maximized;
        SystemDecorations = msg.IsFullScreen ? SystemDecorations.None : SystemDecorations.Full;
    });
```

3. Liez la touche F11 :

```xml
<Window.KeyBindings>
    <KeyBinding Key="F11" Command="{Binding ToggleFullScreenCommand}"/>
</Window.KeyBindings>
```

## Options Disponibles

| Mode | WindowState | SystemDecorations | Description |
|------|-------------|-------------------|-------------|
| **Normal** | `Normal` | `Full` | Fenêtre standard avec bordures |
| **Maximisé** | `Maximized` | `Full` | Fenêtre maximisée avec barre de titre ✅ **ACTUEL** |
| **Plein Écran** | `FullScreen` | `None` | Mode immersif sans bordures |
| **Borderless** | `Maximized` | `None` | Maximisé sans bordures, barre de tâches visible |

## Raccourcis Clavier Suggérés

- **F11** : Toggle Full Screen
- **Alt+Enter** : Toggle Borderless
- **Esc** : Quitter le plein écran (si activé)

## Notes

- Le mode **Maximized** est le meilleur compromis pour la plupart des utilisateurs
- Le mode **FullScreen** convient aux présentations ou streamers
- Le mode **Borderless** est populaire chez les joueurs (permet Alt+Tab rapide)
