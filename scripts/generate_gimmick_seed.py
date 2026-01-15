"""
Generate comprehensive gimmick seed data SQL file
Creates 1,750+ gimmicks across 7 categories (250+ each)
"""

import random

# Gimmick data structure
categories = {
    "POWER": {
        "subcategories": ["Monster", "Powerhouse", "Giant", "Supernatural", "Savage"],
        "alignment_weights": {"Heel": 0.6, "Face": 0.2, "Tweener": 0.1, "Any": 0.1},
        "entertainment_range": (-5, 10),
        "crowd_range": (5, 15)
    },
    "TECHNICAL": {
        "subcategories": ["Technician", "Strategist", "Ring General", "Submission Expert", "Counter Specialist"],
        "alignment_weights": {"Face": 0.4, "Heel": 0.3, "Tweener": 0.2, "Any": 0.1},
        "entertainment_range": (-10, 5),
        "crowd_range": (-5, 10)
    },
    "HIGHFLYER": {
        "subcategories": ["Aerial Assassin", "Lucha Libre", "Springboard Specialist", "Moonsault Master", "Dive Specialist"],
        "alignment_weights": {"Face": 0.7, "Tweener": 0.2, "Heel": 0.05, "Any": 0.05},
        "entertainment_range": (5, 20),
        "crowd_range": (10, 20)
    },
    "BRAWLER": {
        "subcategories": ["Street Fighter", "Bar Room Brawler", "Knockout Artist", "Heavy Hitter", "Pit Fighter"],
        "alignment_weights": {"Heel": 0.5, "Tweener": 0.3, "Face": 0.1, "Any": 0.1},
        "entertainment_range": (-5, 10),
        "crowd_range": (0, 15)
    },
    "SHOWMAN": {
        "subcategories": ["Charismatic Performer", "Comedy Character", "Flamboyant Superstar", "Trash Talker", "Gimmick Master"],
        "alignment_weights": {"Face": 0.5, "Heel": 0.3, "Tweener": 0.1, "Any": 0.1},
        "entertainment_range": (10, 20),
        "crowd_range": (5, 20)
    },
    "HARDCORE": {
        "subcategories": ["Extreme Warrior", "Pain Dealer", "Deathmatch Icon", "Barbed Wire Specialist", "Hardcore Innovator"],
        "alignment_weights": {"Heel": 0.6, "Tweener": 0.3, "Face": 0.05, "Any": 0.05},
        "entertainment_range": (-10, 15),
        "crowd_range": (5, 20)
    },
    "ALLROUNDER": {
        "subcategories": ["Complete Package", "Hybrid Fighter", "Adaptive Champion", "Style Switcher", "Universal Performer"],
        "alignment_weights": {"Any": 0.4, "Face": 0.3, "Tweener": 0.2, "Heel": 0.1},
        "entertainment_range": (0, 15),
        "crowd_range": (0, 15)
    }
}

# Gimmick name templates by category
gimmick_templates = {
    "POWER": [
        "The {adjective}", "The {noun}", "{title} {noun}", "The {adjective} {noun}",
        "{adjective} {name}", "The {adjective} One", "{noun} Incarnate"
    ],
    "TECHNICAL": [
        "The {adjective}", "The {noun}", "The {adjective} {noun}", "{title} of {noun}",
        "The {noun} Master", "The {adjective} One", "{noun} Specialist"
    ],
    "HIGHFLYER": [
        "The {adjective}", "The {noun}", "The {adjective} {noun}", "{adjective} {name}",
        "The Sky {noun}", "The Aerial {noun}", "{noun} from Above"
    ],
    "BRAWLER": [
        "The {adjective}", "The {noun}", "The {adjective} {noun}", "{adjective} {name}",
        "The Street {noun}", "The {noun} Fighter", "{title} {noun}"
    ],
    "SHOWMAN": [
        "The {adjective}", "The {noun}", "The {adjective} {noun}", "{adjective} {name}",
        "The {adjective} One", "{title} of {noun}", "Mr. {adjective}"
    ],
    "HARDCORE": [
        "The {adjective}", "The {noun}", "The {adjective} {noun}", "{adjective} {name}",
        "The {noun} King", "The Extreme {noun}", "{noun} Incarnate"
    ],
    "ALLROUNDER": [
        "The {adjective}", "The {noun}", "The {adjective} {noun}", "{adjective} {name}",
        "The Complete {noun}", "The {adjective} One", "{title} {noun}"
    ]
}

# Word banks for generation
adjectives = [
    "Mighty", "Supreme", "Ultimate", "Unstoppable", "Invincible", "Legendary", "Immortal", "Eternal",
    "Dark", "Shadow", "Silent", "Deadly", "Fierce", "Savage", "Wild", "Ruthless",
    "Golden", "Silver", "Bronze", "Iron", "Steel", "Diamond", "Platinum", "Titanium",
    "Crimson", "Scarlet", "Azure", "Emerald", "Obsidian", "Ivory", "Onyx", "Jade",
    "Raging", "Furious", "Blazing", "Frozen", "Thunder", "Lightning", "Storm", "Tempest",
    "Ancient", "Primal", "Cosmic", "Divine", "Infernal", "Celestial", "Demonic", "Angelic",
    "Brutal", "Vicious", "Merciless", "Relentless", "Fearless", "Courageous", "Valiant", "Noble"
]

nouns = [
    "Warrior", "Champion", "Destroyer", "Conqueror", "Dominator", "Titan", "Colossus", "Behemoth",
    "Beast", "Monster", "Demon", "Dragon", "Phoenix", "Eagle", "Lion", "Tiger",
    "King", "Emperor", "Lord", "Master", "Legend", "Icon", "Hero", "Villain",
    "Assassin", "Hunter", "Predator", "Reaper", "Executioner", "Gladiator", "Samurai", "Ninja",
    "Thunder", "Lightning", "Storm", "Tempest", "Hurricane", "Tornado", "Cyclone", "Typhoon",
    "Hammer", "Anvil", "Blade", "Sword", "Axe", "Spear", "Shield", "Armor",
    "Force", "Power", "Might", "Strength", "Fury", "Rage", "Wrath", "Vengeance"
]

titles = [
    "Lord", "King", "Emperor", "Master", "Champion", "Prince", "Duke", "Baron",
    "Captain", "General", "Commander", "Chief", "Boss", "Leader", "Ruler", "Sovereign"
]

names = [
    "Doom", "Pain", "Death", "Chaos", "Havoc", "Mayhem", "Terror", "Horror",
    "Glory", "Honor", "Justice", "Valor", "Pride", "Fury", "Rage", "Wrath"
]

def generate_gimmick_name(category, index):
    """Generate a unique gimmick name"""
    template = random.choice(gimmick_templates[category])
    
    name = template.format(
        adjective=random.choice(adjectives),
        noun=random.choice(nouns),
        title=random.choice(titles),
        name=random.choice(names)
    )
    
    # Add number suffix to ensure uniqueness
    return f"{name} #{index % 100 + 1}" if index > 50 else name

def weighted_choice(weights):
    """Choose based on weights"""
    items = list(weights.keys())
    probabilities = list(weights.values())
    return random.choices(items, weights=probabilities)[0]

def generate_gimmicks_for_category(category, count=250):
    """Generate gimmicks for a specific category"""
    config = categories[category]
    gimmicks = []
    
    for i in range(count):
        gimmick_id = f"GIMMICK_{category}_{i+1:03d}"
        name = generate_gimmick_name(category, i)
        subcategory = random.choice(config["subcategories"])
        alignment = weighted_choice(config["alignment_weights"])
        entertainment = random.randint(*config["entertainment_range"])
        crowd = random.randint(*config["crowd_range"])
        
        # Randomly assign popularity tier
        tier_weights = {"Jobber": 0.1, "LowerMid": 0.2, "MidCard": 0.4, "UpperMid": 0.2, "MainEvent": 0.1}
        tier = weighted_choice(tier_weights)
        
        # Randomly assign era
        era_weights = {"Any": 0.6, "Modern": 0.2, "Attitude": 0.1, "Golden": 0.1}
        era = weighted_choice(era_weights)
        
        description = f"A {subcategory.lower()} gimmick from the {category.lower()} category"
        
        gimmicks.append({
            "id": gimmick_id,
            "name": name,
            "description": description,
            "category": category,
            "subcategory": subcategory,
            "entertainment": entertainment,
            "crowd": crowd,
            "alignment": alignment,
            "era": era,
            "tier": tier
        })
    
    return gimmicks

def generate_sql_file():
    """Generate the complete SQL seed file"""
    output = []
    output.append("-- ============================================================================")
    output.append("-- Gimmick Seed Data: 1,750+ Wrestling Gimmicks")
    output.append("-- Generated: 2026-01-15")
    output.append("-- ============================================================================\n")
    
    all_gimmicks = []
    
    for category in categories.keys():
        print(f"Generating {category} gimmicks...")
        gimmicks = generate_gimmicks_for_category(category, 250)
        all_gimmicks.extend(gimmicks)
    
    # Generate INSERT statements in batches
    output.append("-- Insert all gimmicks\n")
    
    batch_size = 50
    for i in range(0, len(all_gimmicks), batch_size):
        batch = all_gimmicks[i:i+batch_size]
        
        output.append("INSERT OR IGNORE INTO Gimmicks (")
        output.append("    GimmickId, Name, Description, Category, SubCategory,")
        output.append("    EntertainmentModifier, CrowdReactionModifier,")
        output.append("    PreferredAlignment, EraCompatibility, PopularityTier")
        output.append(") VALUES")
        
        values = []
        for g in batch:
            escaped_name = g['name'].replace("'", "''")
            value = f"('{g['id']}', '{escaped_name}', '{g['description']}', '{g['category']}', '{g['subcategory']}', {g['entertainment']}, {g['crowd']}, '{g['alignment']}', '{g['era']}', '{g['tier']}')"
            values.append(value)
        
        output.append(",\n".join(values))
        output.append(";\n")
    
    # Add verification query
    output.append("\n-- Verification")
    output.append("SELECT Category, COUNT(*) as Count FROM Gimmicks GROUP BY Category;")
    output.append("SELECT 'Total Gimmicks' as Metric, COUNT(*) as Count FROM Gimmicks;")
    
    return "\n".join(output)

if __name__ == "__main__":
    print("Generating gimmick seed SQL file...")
    sql_content = generate_sql_file()
    
    output_file = "seed_gimmicks.sql"
    with open(output_file, "w", encoding="utf-8") as f:
        f.write(sql_content)
    
    print(f"Generated {output_file} successfully!")
    print(f"File size: {len(sql_content)} bytes")
