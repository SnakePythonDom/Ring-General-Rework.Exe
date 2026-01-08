# 🎨 Sprint 2 - Design des Tabs ProfileView

**Date** : 7 janvier 2026
**Version** : 2.0 (Révisée)

---

## 🎯 Vue d'Ensemble

ProfileView avec **6 onglets** pour afficher les informations complètes d'un Worker/Staff/Trainee.

---

## 📊 TAB 1 : ATTRIBUTS

### Layout Général

```
┌─────────────────────────────────────────────────────────────────┐
│                        FICHE PERSONNAGE                          │
├───────────────────┬─────────────────────────────────────────────┤
│                   │                                             │
│   ┌─────────┐    │  NOM COMPLET                                │
│   │         │    │  John Cena                                  │
│   │  PHOTO  │    │  ─────────────────────────────────────────  │
│   │   ou    │    │  Type: Main Eventer • Rôle TV: Upper Card  │
│   │ AVATAR  │    │  Spécialisations: Brawler, Power           │
│   │ 200x200 │    │  ─────────────────────────────────────────  │
│   └─────────┘    │  📅 Âge: 46 ans (27 avril 1977)            │
│                   │  🌍 Naissance: West Newbury, USA           │
│   [📁 Changer]   │  🏠 Résidence: Tampa, Floride, USA         │
│   [🎨 Avatar]    │                                             │
│                   │                                             │
└───────────────────┴─────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│  ▾ ATTRIBUTS UNIVERSELS                                         │
├─────────────────────────────────────────────────────────────────┤
│  Condition Physique        78  ↑2                               │
│  ███████████████░░░░░ (Green)                                   │
│                                                                  │
│  Moral                     85                                   │
│  ████████████████░░░ (Green)                                    │
│                                                                  │
│  Popularité                95  ↑5                               │
│  ███████████████████ (Green)                                    │
│                                                                  │
│  Fatigue                   35  ↓8                               │
│  ███████░░░░░░░░░░░░ (Red)                                      │
│                                                                  │
│  Momentum                  88                                   │
│  █████████████████░░ (Green)                                    │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│  ▾ IN-RING                                                      │
├─────────────────────────────────────────────────────────────────┤
│  In-Ring (Moyenne)         82                                   │
│  Timing                    85                                   │
│  Psychology                80                                   │
│  Selling                   78                                   │
│  Stamina                   85                                   │
│  Safety                    90                                   │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│  ▾ ENTERTAINMENT                                                │
├─────────────────────────────────────────────────────────────────┤
│  Entertainment (Moyenne)   88                                   │
│  Charisma                  92                                   │
│  Promo                     90                                   │
│  Crowd Connection          95                                   │
│  Star Power                85                                   │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│  ▾ STORY                                                        │
├─────────────────────────────────────────────────────────────────┤
│  Story (Moyenne)           80                                   │
│  Storytelling              82                                   │
│  Character Work            78                                   │
└─────────────────────────────────────────────────────────────────┘
```

### Spécifications Techniques

**Fiche Personnage** (Header section):

```csharp
public class CharacterSheetViewModel : ViewModelBase
{
    // Photo/Avatar
    public string PhotoPath { get; set; }
    public bool HasCustomPhoto { get; }
    public ReactiveCommand<Unit, Unit> ChangePhotoCommand { get; }
    public ReactiveCommand<Unit, Unit> GenerateAvatarCommand { get; }

    // Identité
    public string FullName { get; }
    public string RingName { get; }

    // Info Rapide
    public string WorkerType { get; } // Main Eventer, Upper Mid-Carder, Mid-Carder, etc.
    public string TvRole { get; } // Upper Card, Mid Card, Lower Card
    public ObservableCollection<string> Specializations { get; } // Brawler, Technical, High-Flyer, Power, etc.

    // Âge et Dates
    public int Age { get; }
    public DateTime BirthDate { get; }
    public string BirthDateFormatted { get; } // "27 avril 1977"

    // Géographie
    public string Birthplace { get; } // Ville, Pays
    public string BirthCountry { get; } // Pays de naissance
    public string Residence { get; } // Ville, État/Province, Pays
    public string ResidenceCountry { get; } // Pays de résidence
}
```

**XAML Structure** :

```xml
<ScrollViewer>
  <StackPanel Spacing="16" Margin="16">
    <!-- FICHE PERSONNAGE -->
    <Border Classes="panel" Padding="20">
      <Grid ColumnDefinitions="220,*">
        <!-- Colonne Photo -->
        <StackPanel Grid.Column="0" Spacing="8">
          <Border Width="200" Height="200" CornerRadius="8" ClipToBounds="True"
                  BorderBrush="#3a3a3a" BorderThickness="2">
            <Image Source="{Binding PhotoPath}"
                   Stretch="UniformToFill"/>
          </Border>
          <Button Classes="secondary" Content="📁 Changer Photo"
                  Command="{Binding ChangePhotoCommand}"/>
          <Button Classes="secondary" Content="🎨 Générer Avatar"
                  Command="{Binding GenerateAvatarCommand}"/>
        </StackPanel>

        <!-- Colonne Infos -->
        <StackPanel Grid.Column="1" Spacing="12">
          <TextBlock Classes="h2" Text="{Binding FullName}"/>
          <Separator Background="#3a3a3a" Height="1"/>

          <!-- Type et Rôle -->
          <WrapPanel>
            <TextBlock Classes="body" Text="{Binding WorkerType}"/>
            <TextBlock Classes="body muted" Text=" • " Margin="4,0"/>
            <TextBlock Classes="body" Text="Rôle TV: "/>
            <TextBlock Classes="body info" Text="{Binding TvRole}"/>
          </WrapPanel>

          <!-- Spécialisations -->
          <WrapPanel>
            <TextBlock Classes="body" Text="Spécialisations: "/>
            <ItemsControl ItemsSource="{Binding Specializations}">
              <ItemsControl.ItemsPanel>
                <ItemsPanelTemplate>
                  <WrapPanel/>
                </ItemsPanelTemplate>
              </ItemsControl.ItemsPanel>
              <ItemsControl.ItemTemplate>
                <DataTemplate>
                  <Border Classes="badge" Background="#3b82f6"
                          Padding="6,2" Margin="4,0" CornerRadius="3">
                    <TextBlock Classes="caption" Foreground="White"
                               Text="{Binding}"/>
                  </Border>
                </DataTemplate>
              </ItemsControl.ItemTemplate>
            </ItemsControl>
          </WrapPanel>

          <Separator Background="#3a3a3a" Height="1"/>

          <!-- Âge et Dates -->
          <StackPanel Orientation="Horizontal" Spacing="6">
            <TextBlock Text="📅" FontSize="16"/>
            <TextBlock Classes="body" Text="Âge: "/>
            <TextBlock Classes="body" FontWeight="Bold"
                       Text="{Binding Age}"/>
            <TextBlock Classes="body" Text=" ans ("/>
            <TextBlock Classes="body muted"
                       Text="{Binding BirthDateFormatted}"/>
            <TextBlock Classes="body" Text=")"/>
          </StackPanel>

          <!-- Naissance -->
          <StackPanel Orientation="Horizontal" Spacing="6">
            <TextBlock Text="🌍" FontSize="16"/>
            <TextBlock Classes="body" Text="Naissance: "/>
            <TextBlock Classes="body" Text="{Binding Birthplace}"/>
          </StackPanel>

          <!-- Résidence -->
          <StackPanel Orientation="Horizontal" Spacing="6">
            <TextBlock Text="🏠" FontSize="16"/>
            <TextBlock Classes="body" Text="Résidence: "/>
            <TextBlock Classes="body" Text="{Binding Residence}"/>
          </StackPanel>
        </StackPanel>
      </Grid>
    </Border>

    <!-- ATTRIBUTS UNIVERSELS -->
    <Expander Header="▾ ATTRIBUTS UNIVERSELS" IsExpanded="True">
      <StackPanel Spacing="6" Margin="0,8,0,0">
        <components:AttributeBar
          AttributeName="Condition Physique"
          Value="{Binding ConditionPhysique}"
          PreviousValue="{Binding PreviousConditionPhysique}"/>
        <!-- ... autres attributs ... -->
      </StackPanel>
    </Expander>

    <!-- IN-RING -->
    <Expander Header="▾ IN-RING" IsExpanded="True"
              IsVisible="{Binding IsWorker}">
      <!-- ... -->
    </Expander>

    <!-- ENTERTAINMENT -->
    <Expander Header="▾ ENTERTAINMENT" IsExpanded="True"
              IsVisible="{Binding IsWorker}">
      <!-- ... -->
    </Expander>

    <!-- STORY -->
    <Expander Header="▾ STORY" IsExpanded="True"
              IsVisible="{Binding IsWorker}">
      <!-- ... -->
    </Expander>
  </StackPanel>
</ScrollViewer>
```

### Spécialisations Possibles

**Styles de Combat** :
- 🥊 Brawler - Combat brutal et physique
- 🤸 High-Flyer - Style aérien et acrobatique
- 💪 Power - Force brute et slams
- 🎯 Technical - Lutte technique et mat wrestling
- 🔪 Hardcore - Armes et matchs extrêmes
- 🤼 Submission - Prises de soumission
- 🎭 Showman - Entertaineur spectaculaire

---

## 👥 TAB 4 : RELATIONS (Révisé)

### Layout Général

```
┌─────────────────────────────────────────────────────────────────┐
│  Relations avec les autres workers               [+ Ajouter]    │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │ 🤝  Randy Orton                                    ✏ 🗑  │  │
│  │     Amitié • Fort (85/100)                                │  │
│  │     Tag team partner depuis 2020. Bonne chimie.          │  │
│  └──────────────────────────────────────────────────────────┘  │
│                                                                  │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │ ⚔  The Rock                                       ✏ 🗑  │  │
│  │     Rivalité • Très Fort (95/100)                         │  │
│  │     Feud historique. Chemistry exceptionnelle.            │  │
│  └──────────────────────────────────────────────────────────┘  │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│  Factions et Équipes                              [+ Créer]     │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │ 👊  The Shield                                    ✏ 🗑  │  │
│  │     Type: Faction (3+ membres)                            │  │
│  │     Membres: John Cena, Randy Orton, CM Punk              │  │
│  │     Leader: John Cena                                     │  │
│  │     Status: Active • Créée: Semaine 12/2023              │  │
│  └──────────────────────────────────────────────────────────┘  │
│                                                                  │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │ 🤜🤛  The Brothers of Destruction                 ✏ 🗑  │  │
│  │     Type: Tag Team (2 membres)                            │  │
│  │     Membres: John Cena, Randy Orton                       │  │
│  │     Status: Active • Créée: Semaine 24/2022              │  │
│  └──────────────────────────────────────────────────────────┘  │
│                                                                  │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │ 🎯  The Elite Trio                                ✏ 🗑  │  │
│  │     Type: Trio (3 membres)                                │  │
│  │     Membres: John Cena, Randy Orton, Edge                 │  │
│  │     Status: Inactive • Créée: Semaine 8/2021              │  │
│  └──────────────────────────────────────────────────────────┘  │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

### Spécifications Techniques

**Types de Relations (1-à-1)** :
- 🤝 **Amitié** (Friendship)
- ❤ **Couple** (Romantic)
- 👊 **Fraternité** (Brotherhood)
- ⚔ **Rivalité** (Rivalry)

**Types de Factions (1-à-plusieurs)** :
- 🤜🤛 **Tag Team** (2 membres)
- 🎯 **Trio** (3 membres)
- 👊 **Faction** (3+ membres, typiquement 4-6)

```csharp
// Relations ViewModels
public class RelationsTabViewModel : ViewModelBase
{
    // Relations 1-à-1
    public ObservableCollection<WorkerRelationViewModel> Relations { get; }
    public ReactiveCommand<Unit, Unit> AddRelationCommand { get; }

    // Factions
    public ObservableCollection<FactionViewModel> Factions { get; }
    public ReactiveCommand<Unit, Unit> CreateFactionCommand { get; }
}

public class WorkerRelationViewModel : ViewModelBase
{
    public string RelatedWorkerId { get; }
    public string RelatedWorkerName { get; }
    public RelationType RelationType { get; } // Amitié, Couple, Fraternité, Rivalité
    public string RelationTypeIcon { get; } // 🤝 ❤ 👊 ⚔
    public int RelationStrength { get; set; } // 0-100
    public string RelationStrengthText { get; } // "Faible", "Moyen", "Fort", "Très Fort"
    public bool IsStrongRelation { get; } // >= 70
    public bool IsMediumRelation { get; } // 40-69
    public string Notes { get; set; }
    public bool IsPublic { get; set; } // Kayfabe vs Backstage
}

public class FactionViewModel : ViewModelBase
{
    public string FactionId { get; }
    public string FactionName { get; set; }
    public FactionType FactionType { get; } // TagTeam, Trio, Faction
    public string FactionTypeIcon { get; } // 🤜🤛 🎯 👊
    public ObservableCollection<string> MemberIds { get; }
    public ObservableCollection<string> MemberNames { get; }
    public string LeaderId { get; set; } // Optionnel
    public string LeaderName { get; }
    public FactionStatus Status { get; set; } // Active, Inactive, Disbanded
    public int CreatedWeek { get; }
    public int CreatedYear { get; }
    public string CreatedDateText { get; } // "Semaine 12/2023"

    public ReactiveCommand<Unit, Unit> EditFactionCommand { get; }
    public ReactiveCommand<Unit, Unit> DisbandFactionCommand { get; }
    public ReactiveCommand<string, Unit> RemoveMemberCommand { get; }
    public ReactiveCommand<Unit, Unit> AddMemberCommand { get; }
}

public enum RelationType
{
    Amitie,      // 🤝
    Couple,      // ❤
    Fraternite,  // 👊
    Rivalite     // ⚔
}

public enum FactionType
{
    TagTeam,  // 🤜🤛 (2 membres)
    Trio,     // 🎯 (3 membres)
    Faction   // 👊 (3+ membres, généralement 4-6)
}

public enum FactionStatus
{
    Active,
    Inactive,
    Disbanded
}
```

**XAML Structure Relations Tab** :

```xml
<ScrollViewer>
  <StackPanel Spacing="20" Margin="16">
    <!-- RELATIONS 1-à-1 -->
    <StackPanel>
      <Grid ColumnDefinitions="*,Auto" Margin="0,0,0,12">
        <TextBlock Classes="h3"
                   Text="Relations avec les autres workers"
                   VerticalAlignment="Center"/>
        <Button Grid.Column="1" Classes="primary"
                Content="+ Ajouter"
                Command="{Binding AddRelationCommand}"/>
      </Grid>

      <ItemsControl ItemsSource="{Binding Relations}">
        <ItemsControl.ItemTemplate>
          <DataTemplate>
            <Border Classes="card" Margin="0,8">
              <Grid ColumnDefinitions="Auto,*,Auto">
                <!-- Icône -->
                <TextBlock Grid.Column="0" FontSize="32"
                           Text="{Binding RelationTypeIcon}"
                           VerticalAlignment="Center" Margin="0,0,12,0"/>

                <!-- Infos -->
                <StackPanel Grid.Column="1">
                  <TextBlock Classes="body" FontWeight="SemiBold"
                             Text="{Binding RelatedWorkerName}"/>
                  <StackPanel Orientation="Horizontal" Spacing="8">
                    <TextBlock Classes="caption muted"
                               Text="{Binding RelationType}"/>
                    <TextBlock Classes="caption muted" Text="•"/>
                    <TextBlock Classes="caption"
                               Classes.success="{Binding IsStrongRelation}"
                               Classes.warning="{Binding IsMediumRelation}"
                               Text="{Binding RelationStrengthText}"/>
                    <TextBlock Classes="caption muted"
                               Text="{Binding RelationStrength, StringFormat='({0}/100)'}"/>
                  </StackPanel>
                  <TextBlock Classes="caption muted"
                             Text="{Binding Notes}"
                             TextWrapping="Wrap" Margin="0,4,0,0"/>
                </StackPanel>

                <!-- Actions -->
                <StackPanel Grid.Column="2" Orientation="Horizontal" Spacing="4">
                  <Button Classes="icon" Content="✏"/>
                  <Button Classes="icon" Content="🗑"/>
                </StackPanel>
              </Grid>
            </Border>
          </DataTemplate>
        </ItemsControl.ItemTemplate>
      </ItemsControl>
    </StackPanel>

    <!-- FACTIONS -->
    <StackPanel>
      <Grid ColumnDefinitions="*,Auto" Margin="0,0,0,12">
        <TextBlock Classes="h3"
                   Text="Factions et Équipes"
                   VerticalAlignment="Center"/>
        <Button Grid.Column="1" Classes="primary"
                Content="+ Créer"
                Command="{Binding CreateFactionCommand}"/>
      </Grid>

      <ItemsControl ItemsSource="{Binding Factions}">
        <ItemsControl.ItemTemplate>
          <DataTemplate>
            <Border Classes="card" Margin="0,8">
              <Grid ColumnDefinitions="Auto,*,Auto">
                <!-- Icône -->
                <TextBlock Grid.Column="0" FontSize="32"
                           Text="{Binding FactionTypeIcon}"
                           VerticalAlignment="Center" Margin="0,0,12,0"/>

                <!-- Infos -->
                <StackPanel Grid.Column="1">
                  <TextBlock Classes="body" FontWeight="SemiBold"
                             Text="{Binding FactionName}"/>

                  <StackPanel Orientation="Horizontal" Spacing="8" Margin="0,2,0,0">
                    <TextBlock Classes="caption muted" Text="Type: "/>
                    <TextBlock Classes="caption" Text="{Binding FactionType}"/>
                    <TextBlock Classes="caption muted"
                               Text="{Binding MemberIds.Count, StringFormat='({0} membres)'}"/>
                  </StackPanel>

                  <TextBlock Classes="caption muted" Margin="0,2,0,0">
                    <Run Text="Membres: "/>
                    <Run FontWeight="Medium"
                         Text="{Binding MemberNamesText}"/>
                  </TextBlock>

                  <StackPanel Orientation="Horizontal" Spacing="8" Margin="0,2,0,0"
                              IsVisible="{Binding HasLeader}">
                    <TextBlock Classes="caption muted" Text="Leader: "/>
                    <TextBlock Classes="caption" FontWeight="Medium"
                               Text="{Binding LeaderName}"/>
                  </StackPanel>

                  <StackPanel Orientation="Horizontal" Spacing="8" Margin="0,4,0,0">
                    <Border Classes="badge"
                            Background="{Binding StatusColor}"
                            Padding="6,2" CornerRadius="3">
                      <TextBlock Classes="caption" Foreground="White"
                                 Text="{Binding Status}"/>
                    </Border>
                    <TextBlock Classes="caption muted" Text="•"/>
                    <TextBlock Classes="caption muted"
                               Text="{Binding CreatedDateText, StringFormat='Créée: {0}'}"/>
                  </StackPanel>
                </StackPanel>

                <!-- Actions -->
                <StackPanel Grid.Column="2" Orientation="Horizontal" Spacing="4">
                  <Button Classes="icon" Content="✏"
                          Command="{Binding EditFactionCommand}"/>
                  <Button Classes="icon" Content="🗑"
                          Command="{Binding DisbandFactionCommand}"/>
                </StackPanel>
              </Grid>
            </Border>
          </DataTemplate>
        </ItemsControl.ItemTemplate>
      </ItemsControl>
    </StackPanel>
  </StackPanel>
</ScrollViewer>
```

---

## 📝 Autres Tabs (Résumé)

### TAB 2 : CONTRATS
- Dates contrat (début, fin, semaines restantes)
- Salaire (hebdomadaire, bonus signing)
- Type de contrat (Exclusive, Per-Appearance, Developmental)
- Options (Auto-renew, Release clause)
- Historique des contrats
- Actions: Renégocier, Prolonger, Libérer

### TAB 3 : GIMMICK/PUSH
- Gimmick actuel (éditable)
- Alignment (Face, Heel, Tweener)
- Push Level (Main Event, Upper Mid, Mid, Lower, Jobber)
- TV Role (0-100 scale)
- Booking Intent (notes du booker)
- Finishers et Signatures
- Historique des gimmicks

### TAB 5 : HISTORIQUE/BIOGRAPHIE
- Biographie (nom réel, date naissance, hometown, taille, poids)
- Dates carrière (début carrière, arrivée compagnie)
- Historique des titres (régnés)
- Historique des matchs (récent)
- Historique des blessures
- Historique des storylines
- Statistiques (W/L, %, titres)

### TAB 6 : NOTES
- Liste des notes avec catégories
- Catégories: Booking Ideas, Personal, Injury, Other
- Add/Edit/Delete notes
- Timestamps automatiques

---

## 🎨 Palette de Couleurs

**Status Colors** :
- Active Faction: `#10b981` (Green)
- Inactive Faction: `#f59e0b` (Orange)
- Disbanded Faction: `#666666` (Gray)

**Relation Strength** :
- Faible (0-39): `#ef4444` (Red)
- Moyen (40-69): `#f59e0b` (Orange)
- Fort (70-89): `#10b981` (Green)
- Très Fort (90-100): `#3b82f6` (Blue)

---

## 📐 Dimensions

**Photo/Avatar** :
- Taille: 200x200px
- Border radius: 8px
- Border: 2px solid #3a3a3a

**Badges** :
- Padding: 6px horizontal, 2px vertical
- Border radius: 3px
- Font size: 11px (caption)

**Cards** :
- Margin: 0px top/bottom, 8px between
- Padding: 12px
- Border radius: 6px

---

**Document créé le 7 janvier 2026**
**Prêt pour approbation avant implémentation**
