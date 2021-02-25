using System;
using System.Data;
using System.IO;
using System.Data.SqlClient;

namespace LabelMaker
{
    class SQLBackup
    {
        private string backup_location;
        private string connection_string;
        private string operation;
        private bool overwrite;

        public static bool[] DatabaseBackup(string[] args)
        {
            SQLBackup backup = new SQLBackup(args[0], args[2], args[1]);
            bool[] success = new bool[args.Length - 4];

            for (int i = 4; i < args.Length; i++)
            {
                if (backup.operation == "restore")
                {
                    Console.WriteLine("Restoring table {0} (overwrite={1})", args[i], backup.overwrite);
                    success[i - 4] = backup.restore_table(args[i]);
                }
                else
                {
                    Console.WriteLine("Backing up table {0}", args[i]);
                    success[i - 4] = backup.backup_table(args[i]);
                }
            }

            return success;
        }

        public void backupUtility(string connectionString, string backupDirectory, string action)
        {
            
        }

        private SQLBackup(string op_type, string backup_dir, string connection_string)
        {
            this.connection_string = connection_string;
            this.backup_location = backup_dir.Replace(@"\", @"/");
            if (!this.backup_location.EndsWith('/'))
            {
                this.backup_location += '/';
            }
            Directory.CreateDirectory(this.backup_location);

            switch (op_type)
            {
                case "Backup":
                    this.operation = "backup";
                    this.overwrite = false;
                    break;
                case "RestoreAll":
                    this.operation = "restore";
                    this.overwrite = true;
                    break;
                case "SelectiveAdditive":
                    this.operation = "restore";
                    this.overwrite = false;
                    break;
                case "SelectiveOverwrite":
                    this.operation = "restore";
                    this.overwrite = true;
                    break;
                default:
                    this.operation = "backup";
                    this.overwrite = false;
                    break;
            }
        }

        private string insert_statement(string schema, string table_name, DataRow data, DataColumnCollection cols)
        {
            string[] columns = new string[cols.Count];
            string[] values = new string[cols.Count];

            for (int i = 0; i < cols.Count; i++)
            {
                columns[i] = cols[i].ColumnName.ToString();
                if (data.IsNull(cols[i]))
                {
                    values[i] = "NULL";
                }
                else
                {
                    values[i] = data[cols[i]].ToString();
                    values[i] = String.Format("'{0}'", values[i].Replace("'", "''"));
                }
            }

            string output = String.Format("INSERT INTO [{0}].[{1}] (\n  ", schema, table_name);
            output += String.Join(", ", columns);
            output += "\n) VALUES (\n  ";
            output += String.Join(", ", values);
            output += "\n);";

            return output;
        }
        private string insert_where_not_exists(string schema, string table_name, DataRow data, DataColumnCollection cols, string pk_col)
        {
            string[] columns = new string[cols.Count];
            string[] values = new string[cols.Count];
            string pk_value = data[pk_col].ToString();

            for (int i = 0; i < cols.Count; i++)
            {
                columns[i] = cols[i].ColumnName.ToString();
                if (data.IsNull(cols[i]))
                {
                    values[i] = "NULL";
                }
                else
                {
                    values[i] = data[cols[i]].ToString();
                    values[i] = String.Format("'{0}'", values[i].Replace("'", "''"));
                }
            }

            string output = "IF NOT EXISTS (\n";
            output += String.Format("  SELECT * FROM [{0}].[{1}]\n", schema, table_name);
            output += String.Format("  WHERE [{0}] = '{1}'\n", pk_col, pk_value);
            output += ")\nBEGIN\n";
            output += String.Format("  INSERT INTO [{0}].[{1}] (\n  ", schema, table_name);
            output += String.Join(", ", columns);
            output += "\n  ) VALUES (\n    ";
            output += String.Join(", ", values);
            output += "\n  )\nEND";

            return output;
        }
        private bool backup_table(string table_name)
        {
            try
            {
                SQLTable table_info = new SQLTable(backup_location, table_name);

                SqlConnection conn = new SqlConnection(connection_string);
                conn.Open();
                SqlCommand command = new SqlCommand("SELECT * FROM " + table_name + ";", conn);
                DataTable data = new DataTable();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    data.Load(reader);
                }

                command = new SqlCommand(String.Format(@"
                SELECT 
                    ORDINAL_POSITION,
                    TABLE_SCHEMA,
                    TABLE_NAME,
                    COLUMN_NAME,
                    DATA_TYPE,
                    CHARACTER_MAXIMUM_LENGTH,
                    COLUMN_DEFAULT,
                    IS_NULLABLE
                FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_NAME = '{0}';", table_name), conn);
                DataTable col_data = new DataTable();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    col_data.Load(reader);
                }
                string schema = col_data.Rows[0]["TABLE_SCHEMA"].ToString();
                Console.WriteLine(schema);

                command = new SqlCommand(String.Format(@"
                SELECT  
                    fk.name 'CONSTRAINT_NAME',
                    c1.name 'COLUMN_NAME',
                    OBJECT_NAME(fk.referenced_object_id) 'REFERENCES',
                    c2.name 'REFERENCES_COLUMN'
                FROM 
                    sys.foreign_keys fk
                INNER JOIN 
                    sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
                INNER JOIN
                    sys.columns c1 ON fkc.parent_column_id = c1.column_id AND fkc.parent_object_id = c1.object_id
                INNER JOIN
                    sys.columns c2 ON fkc.referenced_column_id = c2.column_id AND fkc.referenced_object_id = c2.object_id
                WHERE OBJECT_NAME(fk.parent_object_id) = '{0}';", table_name), conn);
                DataTable fk_data = new DataTable();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    fk_data.Load(reader);
                }

                command = new SqlCommand(String.Format(@"
                SELECT 
                    CONSTRAINT_NAME,
                    COLUMN_NAME
                FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
                WHERE OBJECTPROPERTY(OBJECT_ID(CONSTRAINT_SCHEMA + '.' + QUOTENAME(CONSTRAINT_NAME)), 'IsPrimaryKey') = 1
                AND TABLE_NAME = '{0}';", table_name), conn);
                DataTable pk_data = new DataTable();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    pk_data.Load(reader);
                }

                command = new SqlCommand(String.Format(@"
                SELECT 
                    ccu.CONSTRAINT_NAME,
                    ccu.COLUMN_NAME
                FROM INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE as ccu
                INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS as tc on
                    tc.CONSTRAINT_NAME = ccu.CONSTRAINT_NAME and
                    tc.CONSTRAINT_TYPE = 'UNIQUE'
                WHERE ccu.TABLE_NAME = '{0}';", table_name), conn);
                DataTable uq_data = new DataTable();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    uq_data.Load(reader);
                }
                conn.Close();

                // create overwrite restore script
                using (StreamWriter outputFile = new StreamWriter(table_info.overwrite_file_path))
                {

                    outputFile.WriteLine("-- Remove existing table");
                    outputFile.WriteLine("DROP TABLE IF EXISTS [{0}].[{1}];", schema, table_name);
                    outputFile.WriteLine();

                    outputFile.WriteLine("-- Create table");
                    outputFile.WriteLine("CREATE TABLE [{0}].[{1}] (", schema, table_name);
                    // write statement for each column to be made
                    int i = 0;
                    foreach (DataRow row in col_data.Rows)
                    {
                        i++;
                        string col_name = row["COLUMN_NAME"].ToString();
                        outputFile.Write("    ");
                        // write name of column to add
                        outputFile.Write("[{0}] ", col_name);
                        // write type
                        outputFile.Write("[{0}]", row["DATA_TYPE"]);
                        if (row["CHARACTER_MAXIMUM_LENGTH"].ToString() != "")
                        {
                            outputFile.Write("({0})", row["CHARACTER_MAXIMUM_LENGTH"]);
                        }
                        outputFile.Write(" ");
                        // write auto-increment data
                        if (data.Columns[data.Columns.IndexOf(col_name)].AutoIncrement)
                        {
                            outputFile.Write("IDENTITY({0},{1}) ",
                                data.Columns[data.Columns.IndexOf(col_name)].AutoIncrementSeed,
                                data.Columns[data.Columns.IndexOf(col_name)].AutoIncrementStep
                            );
                        }
                        // write if column is nullable
                        outputFile.Write(row["IS_NULLABLE"].ToString() == "YES" ? "NULL " : "NOT NULL ");
                        // write default if applicable
                        if (row["COLUMN_DEFAULT"].ToString() != "")
                        {
                            outputFile.Write("DEFAULT {0} ", row["COLUMN_DEFAULT"]);
                        }
                        // write unique if applicable
                        outputFile.Write(find_item(uq_data.Rows, "COLUMN_NAME", row["COLUMN_NAME"].ToString()) == -1 ? "" : "UNIQUE");

                        outputFile.WriteLine((i < col_data.Rows.Count) || (pk_data.Rows.Count > 0) ? "," : "");
                    }
                    // add primary keys
                    i = 0;
                    foreach (DataRow pk in pk_data.Rows)
                    {
                        i++;
                        outputFile.Write("    CONSTRAINT [{0}] PRIMARY KEY CLUSTERED ([{1}] ASC)", pk["CONSTRAINT_NAME"], pk["COLUMN_NAME"]);
                        outputFile.WriteLine(i < pk_data.Rows.Count ? "," : "");
                    }
                    outputFile.WriteLine(");");

                    // set this flag to allow keeping ids the same
                    outputFile.WriteLine();
                    outputFile.WriteLine("SET IDENTITY_INSERT [{0}].[{1}] ON;", schema, table_name);

                    foreach (DataRow fk in fk_data.Rows)
                    {
                        outputFile.WriteLine();
                        outputFile.WriteLine("ALTER TABLE [{0}].[{1}]", schema, table_name);
                        outputFile.WriteLine("    ADD CONSTRAINT [{0}]", fk["CONSTRAINT_NAME"]);
                        outputFile.WriteLine("    FOREIGN KEY ([{0}]) REFERENCES [{1}].[{2}] ([{3}]);",
                            fk["COLUMN_NAME"], schema, fk["REFERENCES"], fk["REFERENCES_COLUMN"]
                        );
                    }

                    outputFile.WriteLine();
                    outputFile.WriteLine("-- Add rows");

                    foreach (DataRow row in data.Rows)
                    {
                        outputFile.WriteLine(insert_statement(
                            schema,
                            table_name,
                            row,
                            data.Columns
                        ));
                    }

                    // set this flag off again
                    outputFile.WriteLine();
                    outputFile.WriteLine("SET IDENTITY_INSERT [{0}].[{1}] OFF;", schema, table_name);

                }



                // create additive restore script
                using (StreamWriter outputFile = new StreamWriter(table_info.additive_file_path))
                {
                    // set this flag to allow keeping ids the same
                    outputFile.WriteLine("SET IDENTITY_INSERT [{0}].[{1}] ON;", schema, table_name);
                    outputFile.WriteLine();

                    foreach (DataRow row in data.Rows)
                    {
                        if (pk_data.Rows.Count > 0)
                        {
                            outputFile.WriteLine(insert_where_not_exists(
                                schema,
                                table_name,
                                row,
                                data.Columns,
                                pk_data.Rows[0]["COLUMN_NAME"].ToString()
                            ));
                            outputFile.WriteLine();
                        }
                        else
                        {
                            outputFile.WriteLine(insert_statement(
                                schema,
                                table_name,
                                row,
                                data.Columns
                            ));
                        }
                    }

                    // set this flag off again
                    outputFile.WriteLine();
                    outputFile.WriteLine("SET IDENTITY_INSERT [{0}].[{1}] OFF;", schema, table_name);
                }

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private int find_item(DataRowCollection rows, string col_name, string val)
        {
            for (int i = 0; i < rows.Count; i++)
            {
                if (rows[i][col_name].ToString() == val)
                {
                    return i;
                }
            }
            return -1;
        }

        private bool restore_table(string table_name)
        {
            try
            {
                SQLTable table_info = new SQLTable(backup_location, table_name);
                string filename = overwrite ? table_info.overwrite_file_path : table_info.additive_file_path;

                FileInfo file = new FileInfo(filename);
                string script = file.OpenText().ReadToEnd();

                SqlConnection conn = new SqlConnection(connection_string);
                conn.Open();
                SqlCommand command = new SqlCommand(script, conn);

                command.ExecuteNonQuery();

                conn.Close();

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public SQLTable[] get_tables()
        {
            SqlConnection conn = new SqlConnection(connection_string);
            conn.Open();

            DataTable table = conn.GetSchema("Tables", new string[] { null, null, null, "BASE TABLE" });

            SQLTable[] table_array = new SQLTable[table.Rows.Count];

            int i = 0;
            foreach (DataRow row in table.Rows)
            {
                string table_name = row["TABLE_NAME"].ToString();
                table_array[i] = new SQLTable(backup_location, table_name);
                i++;
            }

            conn.Close();

            return table_array;
        }
    }

    class SQLTable
    {
        public readonly string name;
        public readonly bool has_backup;
        public readonly string overwrite_file_path;
        public readonly string additive_file_path;

        public SQLTable(string backup_dir, string name)
        {
            this.name = name;
            //this.file_path = backup_dir+name+".sql";
            this.overwrite_file_path = backup_dir + name + "_RestoreOverwrite.sql";
            this.additive_file_path = backup_dir + name + "_RestoreAdditive.sql";
            this.has_backup = File.Exists(this.additive_file_path) && File.Exists(this.overwrite_file_path);
        }
    }
}