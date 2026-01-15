namespace RingGeneral.Core.Models
{
    /// <summary>
    /// Personality profiles automatically detected based on mental attributes.
    /// Visible to the player as labels/archetypes.
    /// Inspired by Football Manager's personality system.
    /// </summary>
    public enum PersonalityProfile
    {
        // =====================================================================
        // LES ÉLITES - High Professionalism, High Pressure Handling
        // =====================================================================

        /// <summary>
        /// ⭐ Professionnel Exemplaire
        /// Requirements: Professionnalisme 17+, Sportivité 15+, Tempérament 15+
        /// The model professional. Reliable, respectful, calm under pressure.
        /// </summary>
        ProfessionnelExemplaire,

        /// <summary>
        /// 🏆 Citoyen Modèle
        /// Requirements: Loyauté 17+, Professionnalisme 15+, Égoïsme &lt;6
        /// Locker room pillar. Loyal, selfless, puts company first.
        /// </summary>
        CitoyenModele,

        /// <summary>
        /// 💪 Déterminé
        /// Requirements: Détermination 17+, Pression 15+
        /// Never gives up. Thrives in adversity and big matches.
        /// </summary>
        Déterminé,

        // =====================================================================
        // LES STARS À ÉGO - High Ambition + High Égoïsme
        // =====================================================================

        /// <summary>
        /// 🚀 Ambitieux
        /// Requirements: Ambition 17+, Détermination 13+, Égoïsme 10+
        /// Driven to reach the main event. Hungry for success and titles.
        /// </summary>
        Ambitieux,

        /// <summary>
        /// 👑 Leader de Vestiaire
        /// Requirements: Influence 17+, Professionnalisme 13+, Tempérament 13+
        /// Locker room general. Commands respect, guides younger talent.
        /// </summary>
        LeaderDeVestiaire,

        /// <summary>
        /// 💰 Mercenaire
        /// Requirements: Loyauté &lt;6, Ambition 13+, Égoïsme 13+
        /// Follows the money. No company loyalty, will leave for better offers.
        /// </summary>
        Mercenaire,

        // =====================================================================
        // LES INSTABLES - Low Tempérament or Low Pressure
        // =====================================================================

        /// <summary>
        /// 🔥 Tempérament de Feu
        /// Requirements: Tempérament &lt;6, Professionnalisme &gt;10
        /// Explosive but talented. Backstage incident risk, but delivers in-ring.
        /// </summary>
        TempéramentDeFeu,

        /// <summary>
        /// 🎲 Franc-Tireur
        /// Requirements: Adaptabilité 15+, Tempérament &lt;8, Sportivité &lt;8
        /// Unpredictable loose cannon. Creative but chaotic.
        /// </summary>
        FrancTireur,

        /// <summary>
        /// 📉 Inconstant
        /// Requirements: Pression &lt;8, Détermination &lt;8
        /// Erratic performances. Can't be relied on in big moments.
        /// </summary>
        Inconstant,

        // =====================================================================
        // LES TOXIQUES - High Égoïsme, Low Professionalism
        // =====================================================================

        /// <summary>
        /// 😈 Égoïste
        /// Requirements: Égoïsme 17+, Sportivité &lt;6
        /// Refuses to put others over. All about personal glory.
        /// </summary>
        Égoïste,

        /// <summary>
        /// 👸 Diva
        /// Requirements: Égoïsme 17+, Tempérament &lt;6, Professionnalisme &lt;10
        /// Constant backstage drama. Massive ego, poor temperament.
        /// </summary>
        Diva,

        /// <summary>
        /// 💤 Paresseux
        /// Requirements: Professionnalisme &lt;6, Détermination &lt;6
        /// Lazy, minimum effort. Doesn't care about improvement.
        /// </summary>
        Paresseux,

        // =====================================================================
        // LES STRATÈGES - High Experience-correlated traits
        // =====================================================================

        /// <summary>
        /// 🦊 Vétéran Rusé
        /// Requirements: Adaptabilité 15+, Influence 13+, Sportivité &lt;10
        /// Political backstage operator. Knows how to work the system.
        /// </summary>
        VétéranRusé,

        /// <summary>
        /// 📖 Maître du Storytelling
        /// Requirements: Adaptabilité 17+, Professionnalisme 13+, Pression 13+
        /// Master of in-ring psychology. Crafts compelling narratives.
        /// </summary>
        MaîtreDuStorytelling,

        /// <summary>
        /// 🎭 Politicien
        /// Requirements: Influence 17+, Égoïsme 13+, Tempérament 13+
        /// Backstage power player. Pulls strings behind the scenes.
        /// </summary>
        Politicien,

        // =====================================================================
        // LES BÊTES DE COMPÉTITION - High Determination + Professionalism
        // =====================================================================

        /// <summary>
        /// 🥊 Accro au Ring
        /// Requirements: Détermination 17+, Professionnalisme 15+, Ambition 13+
        /// Lives to wrestle. Would work every night if possible.
        /// </summary>
        AccroAuRing,

        /// <summary>
        /// 🛡️ Pilier Fiable
        /// Requirements: Loyauté 17+, Pression 15+, Professionnalisme 13+
        /// Company cornerstone. Always there when needed.
        /// </summary>
        PilierFiable,

        /// <summary>
        /// ⚙️ Machine de Guerre
        /// Requirements: Détermination 18+, Pression 17+, Tempérament 15+
        /// Indestructible workhorse. Never breaks, never quits.
        /// </summary>
        MachineDeGuerre,

        // =====================================================================
        // LES CRÉATURES MÉDIATIQUES - High Ambition, Variable Professionalism
        // =====================================================================

        /// <summary>
        /// 📸 Obsédé par l'Image
        /// Requirements: Ambition 15+, Égoïsme 15+, Professionnalisme &lt;10
        /// Wants celebrity status more than wrestling excellence.
        /// </summary>
        ObsédéParLImage,

        /// <summary>
        /// ⚡ Charismatique Imprévisible
        /// Requirements: Adaptabilité 15+, Tempérament &lt;8, Ambition 13+
        /// Wild card with natural charisma. Brilliant or disaster.
        /// </summary>
        CharismatiqueImprévisible,

        /// <summary>
        /// 🌟 Aimant à Public
        /// Requirements: Sportivité 17+, Professionnalisme 15+, Tempérament 13+
        /// Natural connection with crowds. Fan favorite energy.
        /// </summary>
        AimantÀPublic,

        // =====================================================================
        // LES PROFILS DANGEREUX - Red Flag Personalities
        // =====================================================================

        /// <summary>
        /// 🐍 Saboteur Passif
        /// Requirements: Sportivité &lt;5, Égoïsme 15+, Influence 10+
        /// Backstabber. Uses influence to sabotage others.
        /// </summary>
        SaboteurPassif,

        /// <summary>
        /// 💥 Instable Chronique
        /// Requirements: Tempérament &lt;5, Pression &lt;5, Professionnalisme &lt;8
        /// Constant risk. Unreliable and volatile.
        /// </summary>
        InstableChronique,

        /// <summary>
        /// ⚠️ Poids Mort
        /// Requirements: Professionnalisme &lt;5, Détermination &lt;5, Ambition &lt;5
        /// No interest in improvement. Dead weight.
        /// </summary>
        PoidsMort,

        // =====================================================================
        // PROFILS PAR DÉFAUT
        // =====================================================================

        /// <summary>
        /// 📊 Équilibré
        /// Requirements: All attributes between 8-13
        /// Standard professional. No standout traits positive or negative.
        /// </summary>
        Équilibré,

        /// <summary>
        /// ❓ Non Déterminé
        /// Profile not yet analyzed or doesn't fit any category.
        /// </summary>
        NonDéterminé
    }
}
