# Assets - Ring General

## Logo / Icône de l'application

Pour ajouter votre logo personnalisé :

### Icône de la fenêtre (icon.png)
1. Placez votre fichier image dans ce dossier : `src/RingGeneral.UI/Assets/icon.png`
2. Format recommandé : PNG, 256x256 pixels
3. Le fichier est déjà référencé dans `MainWindow.axaml` : `Icon="/Assets/icon.png"`

### Logo pour la barre de titre
- Le logo s'affichera automatiquement dans la barre de titre à la place de l'emoji 🎭
- Taille recommandée : 32x32 pixels (pour la barre de titre)

### Fichiers supportés
- **icon.png** : Icône principale (256x256 px)
- **logo.ico** : Format Windows ICO (optionnel)
- **logo-titlebar.png** : Logo pour la barre de titre (32x32 px, optionnel)

### Intégration dans le projet

Les assets doivent être inclus dans le fichier `.csproj` :

```xml
<ItemGroup>
    <AvaloniaResource Include="Assets\**" />
</ItemGroup>
```

Cette configuration est déjà en place si vous utilisez le template Avalonia standard.

## Note

Si le fichier `icon.png` n'existe pas, l'application utilisera l'icône par défaut.
Pour créer rapidement un placeholder, vous pouvez utiliser n'importe quel éditeur d'image ou générateur en ligne.
