-- ============================================================================
-- Personality Data Seed
-- Date: 2026-01-08
-- Description: Seed mental attributes and personalities for existing workers
-- ============================================================================

-- Note: Cette seed sera exécutée par le code C# dans DbSeeder.cs
-- Ce fichier sert de référence pour les valeurs

-- ============================================================================
-- Exemples de personnalités variées
-- ============================================================================

-- JOHN CENA - Professional but Egotistic
-- Mental Attributes:
-- Professionalism: 18, Ambition: 17, Loyalty: 12, Ego: 16, Resilience: 18
-- Adaptability: 14, Creativity: 12, WorkEthic: 19, SocialSkills: 16, Temperament: 13
-- → Résultat: Professional mais égocentrique, traits: Hardworking, Resilient

-- RANDY ORTON - Confident but Arrogant
-- Mental Attributes:
-- Professionalism: 14, Ambition: 15, Loyalty: 10, Ego: 17, Resilience: 16
-- Adaptability: 13, Creativity: 14, WorkEthic: 14, SocialSkills: 8, Temperament: 15
-- → Résultat: Confident but Arrogant, traits: Resilient

-- CM PUNK - Rebellious
-- Mental Attributes:
-- Professionalism: 9, Ambition: 18, Loyalty: 5, Ego: 16, Resilience: 17
-- Adaptability: 15, Creativity: 18, WorkEthic: 16, SocialSkills: 12, Temperament: 8
-- → Résultat: Rebellious, traits: Creative, Resilient

-- DANIEL BRYAN - Loyal Hardworking
-- Mental Attributes:
-- Professionalism: 19, Ambition: 13, Loyalty: 18, Ego: 8, Resilience: 19
-- Adaptability: 17, Creativity: 14, WorkEthic: 20, SocialSkills: 14, Temperament: 16
-- → Résultat: Loyal, traits: Hardworking, Resilient

-- UNDERTAKER - Professional Resilient
-- Mental Attributes:
-- Professionalism: 20, Ambition: 14, Loyalty: 17, Ego: 12, Resilience: 20
-- Adaptability: 15, Creativity: 13, WorkEthic: 19, SocialSkills: 11, Temperament: 18
-- → Résultat: Professional, traits: Resilient, Hardworking

-- ROMAN REIGNS - Ambitious but Volatile
-- Mental Attributes:
-- Professionalism: 13, Ambition: 18, Loyalty: 11, Ego: 16, Resilience: 16
-- Adaptability: 12, Creativity: 11, WorkEthic: 15, SocialSkills: 13, Temperament: 5
-- → Résultat: Ambitious but Volatile, traits: Resilient

-- SETH ROLLINS - Creative Adaptable
-- Mental Attributes:
-- Professionalism: 16, Ambition: 17, Loyalty: 12, Ego: 14, Resilience: 16
-- Adaptability: 18, Creativity: 17, WorkEthic: 17, SocialSkills: 15, Temperament: 12
-- → Résultat: Creative, traits: Adaptable, Hardworking

-- AJ STYLES - Balanced Professional
-- Mental Attributes:
-- Professionalism: 18, Ambition: 14, Loyalty: 16, Ego: 10, Resilience: 17
-- Adaptability: 16, Creativity: 14, WorkEthic: 18, SocialSkills: 13, Temperament: 15
-- → Résultat: Professional, traits: Resilient, Hardworking

-- BROCK LESNAR - Opportunistic
-- Mental Attributes:
-- Professionalism: 11, Ambition: 16, Loyalty: 6, Ego: 18, Resilience: 19
-- Adaptability: 10, Creativity: 8, WorkEthic: 13, SocialSkills: 7, Temperament: 10
-- → Résultat: Opportunistic, traits: Resilient

-- SASHA BANKS - Ambitious
-- Mental Attributes:
-- Professionalism: 15, Ambition: 19, Loyalty: 11, Ego: 15, Resilience: 16
-- Adaptability: 16, Creativity: 17, WorkEthic: 17, SocialSkills: 14, Temperament: 11
-- → Résultat: Ambitious, traits: Creative, Resilient

-- ============================================================================
-- Distribution suggérée pour génération aléatoire
-- ============================================================================

-- Professional / Loyal / Hardworking: 30%
-- Ambitious / Creative / Adaptable: 25%
-- Balanced / Confident: 20%
-- Egotistic / Opportunistic: 15%
-- Volatile / Rebellious / Unmotivated: 10%

-- ============================================================================
-- Notes d'implémentation
-- ============================================================================

-- Le DbSeeder.cs doit:
-- 1. Vérifier si les tables MentalAttributes et Personalities existent
-- 2. Pour chaque Worker existant sans attributs mentaux:
--    a. Générer attributs via PersonalityEngine.GenerateRandomMentalAttributes()
--    b. Calculer label via PersonalityEngine.CalculatePersonalityLabel()
--    c. Générer traits secondaires via PersonalityEngine.GenerateSecondaryTraits()
--    d. Sauvegarder via PersonalityRepository.SaveMentalAttributesAsync()
--    e. Sauvegarder via PersonalityRepository.SavePersonalityAsync()

-- ============================================================================
