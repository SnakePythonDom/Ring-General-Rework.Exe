-- Migration: Add Gender and Specialization preferences to YouthStructures
-- Date: 2026-01-12

-- Add GenderPreference column
ALTER TABLE YouthStructures ADD COLUMN GenderPreference TEXT DEFAULT 'BOTH';

-- Add SpecializationPreference column
ALTER TABLE YouthStructures ADD COLUMN SpecializationPreference TEXT DEFAULT 'NONE';
