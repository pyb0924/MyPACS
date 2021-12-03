SELECT file_path
FROM mypacs_test
WHERE study_instance_UID = :study_instance_uid
  AND series_instance_UID = :series_instance_uid