-- Phase 1.2 - Migration pour ajouter BookingControlLevel à GameState

ALTER TABLE GameState ADD COLUMN BookingControlLevel TEXT NOT NULL DEFAULT 'CoBooker';
