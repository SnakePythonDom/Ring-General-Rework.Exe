-- Migration to refactor WorkerRelations to use TEXT IDs (GUID support)
DROP TABLE IF EXISTS WorkerRelations;

CREATE TABLE WorkerRelations (
    RelationId INTEGER PRIMARY KEY AUTOINCREMENT,
    WorkerId1 TEXT NOT NULL,
    WorkerId2 TEXT NOT NULL,
    RelationType TEXT NOT NULL,
    RelationStrength INTEGER NOT NULL DEFAULT 50,
    Notes TEXT,
    IsPublic INTEGER NOT NULL DEFAULT 1,
    CreatedDate TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (WorkerId1) REFERENCES Workers(WorkerId),
    FOREIGN KEY (WorkerId2) REFERENCES Workers(WorkerId)
);

CREATE INDEX idx_worker_relations_w1 ON WorkerRelations(WorkerId1);
CREATE INDEX idx_worker_relations_w2 ON WorkerRelations(WorkerId2);
