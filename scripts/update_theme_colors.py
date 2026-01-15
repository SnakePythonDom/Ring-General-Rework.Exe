#!/usr/bin/env python3
"""
Script pour remplacer les couleurs codées en dur par les ressources de thème
dans tous les fichiers AXAML.
"""

import os
import re
from pathlib import Path

# Mapping des couleurs vers les ressources de thème
COLOR_MAPPING = {
    # Backgrounds
    '#1a1a1a': '{StaticResource BackgroundPrimaryBrush}',
    '#1e293b': '{StaticResource BackgroundSecondaryBrush}',
    '#0f172a': '{StaticResource BackgroundTertiaryBrush}',
    '#242424': '{StaticResource BackgroundTertiaryBrush}',
    '#252525': '{StaticResource BackgroundSecondaryBrush}',
    '#2d2d2d': '{StaticResource BackgroundInputBrush}',
    '#334155': '{StaticResource BorderMediumBrush}',
    
    # Text colors
    '#e0e0e0': '{StaticResource TextPrimaryBrush}',
    '#888888': '{StaticResource TextSecondaryBrush}',
    '#cbd5e1': '{StaticResource TextSecondaryBrush}',
    '#94a3b8': '{StaticResource TextMutedBrush}',
    '#64748b': '{StaticResource TextSubtleBrush}',
    '#666666': '{StaticResource TextDisabledBrush}',
    '#444444': '{StaticResource TextDark}',
    
    # Accents
    '#3b82f6': '{StaticResource AccentBlueBrush}',
    '#10b981': '{StaticResource AccentGreenBrush}',
    '#f59e0b': '{StaticResource AccentOrangeBrush}',
    '#ef4444': '{StaticResource AccentRedBrush}',
    
    # Success colors
    '#065f46': '{StaticResource SuccessDarkBrush}',
    '#34d399': '{StaticResource SuccessLightBrush}',
    '#d1fae5': '{StaticResource SuccessLightBrush}',
    '#a7f3d0': '{StaticResource SuccessLightBrush}',
    
    # Error colors
    '#7f1d1d': '{StaticResource ErrorDarkBrush}',
    '#fecaca': '{StaticResource ErrorLightBrush}',
    '#fca5a5': '{StaticResource ErrorLightBrush}',
    
    # Other colors
    '#8b5cf6': '{StaticResource BrandSecondaryBrush}',
    '#06b6d4': '{StaticResource BrandAccentBrush}',
    '#ffffff': 'White',
    '#2a2a2a': '{StaticResource BorderDefaultBrush}',
    '#3a3a3a': '{StaticResource BorderLightBrush}',
    '#1e1e1e': '{StaticResource BackgroundSecondaryBrush}',
    '#4a4a4a': '{StaticResource BorderSecondaryBrush}',
    '#4a91ff': '{StaticResource AccentPrimaryHoverBrush}',
    '#333333': '{StaticResource BackgroundHoverBrush}',
    '#b0b0b0': '{StaticResource TextSecondaryBrush}',
    '#1e40af': '{StaticResource BrandPrimaryBrush}',
    '#bfdbfe': '{StaticResource BrandPrimaryBrush}',
    '#93c5fd': '{StaticResource BrandPrimaryBrush}',
    '#713f12': '{StaticResource WarningDarkBrush}',
    '#fde68a': '{StaticResource WarningBrush}',
    '#fbbf24': '{StaticResource WarningBrush}',
    '#7c2d12': '{StaticResource WarningDarkBrush}',
    '#fed7aa': '{StaticResource WarningLightBrush}',
    '#fdba74': '{StaticResource WarningLightBrush}',
    '#0a0a0a': '{StaticResource BackgroundPrimaryBrush}',
    '#2563eb': '{StaticResource BrandPrimaryBrush}',
}

def update_file(file_path):
    """Met à jour un fichier AXAML avec les ressources de thème."""
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        original_content = content
        
        # Remplacer les couleurs
        for color, resource in COLOR_MAPPING.items():
            # Remplacer dans les attributs Background, Foreground, BorderBrush, Fill, etc.
            # et aussi dans les Setter Property="Value"
            patterns = [
                (f'Background="{re.escape(color)}"', f'Background="{resource}"'),
                (f"Background='{re.escape(color)}'", f"Background='{resource}'"),
                (f'Foreground="{re.escape(color)}"', f'Foreground="{resource}"'),
                (f"Foreground='{re.escape(color)}'", f"Foreground='{resource}'"),
                (f'BorderBrush="{re.escape(color)}"', f'BorderBrush="{resource}"'),
                (f"BorderBrush='{re.escape(color)}'", f"BorderBrush='{resource}'"),
                (f'Fill="{re.escape(color)}"', f'Fill="{resource}"'),
                (f"Fill='{re.escape(color)}'", f"Fill='{resource}'"),
                (f'Value="{re.escape(color)}"', f'Value="{resource}"'),
                (f"Value='{re.escape(color)}'", f"Value='{resource}'"),
            ]
            
            for pattern, replacement in patterns:
                content = re.sub(pattern, replacement, content)
        
        # Écrire seulement si le contenu a changé
        if content != original_content:
            with open(file_path, 'w', encoding='utf-8') as f:
                f.write(content)
            return True
        return False
    except Exception as e:
        print(f"Erreur lors du traitement de {file_path}: {e}")
        return False

def main():
    """Point d'entrée principal."""
    # Trouver tous les fichiers AXAML dans Views et Components
    views_dir = Path('src/RingGeneral.UI/Views')
    components_dir = Path('src/RingGeneral.UI/Components')
    axaml_files = list(views_dir.rglob('*.axaml')) + list(components_dir.rglob('*.axaml'))
    
    updated_count = 0
    for file_path in axaml_files:
        if update_file(file_path):
            print(f"[OK] Mis a jour: {file_path}")
            updated_count += 1
        else:
            print(f"  Pas de changement: {file_path}")
    
    print(f"\n{updated_count} fichier(s) mis a jour sur {len(axaml_files)}")

if __name__ == '__main__':
    main()
