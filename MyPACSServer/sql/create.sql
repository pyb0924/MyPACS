CREATE TABLE mypacs_test
(
    sop_instance_uid    VARCHAR(64)  NOT NULL UNIQUE,
    patient_name        VARCHAR(64),
    patient_id          VARCHAR(64)  NOT NULL,
    study_id            VARCHAR(16)  NOT NULL,
    study_instance_uid   VARCHAR(64)  NOT NULL,
    series_description  VARCHAR(64)  NOT NULL,
    series_instance_uid VARCHAR(64)  NOT NULL,
    local_file_path     VARCHAR(256) NOT NULL,
    PRIMARY KEY (sop_instance_uid)
);