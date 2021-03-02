using System;
using System.Data;
using System.IO;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace SQLBackup
{
    class SQLBackup
    {
        /*
        ** Main class for this utility. 
        ** Call SQLBackup.Databasebackup(string[]) to use (see method definition for instructions as to form of paramater)
        */

        // number of backups to keep. backups further back in time are automatically deleted when backup routine is called
        private int keep_backups_count = 10;
        // folder in which backups are stored
        private string backup_location;
        // folder of particular backup to restore from. when backing up this is overwritten
        private string active_folder;
        // connection string for database connection
        private string connection_string;
        // internal. has values "backup" or "restore"
        private string operation;
        // internal. true=delete and remake table, false=add entries that don't exist
        private bool overwrite;
        public static bool[] DatabaseBackup(string[] args)
        {
            /*
            ** Only method needed to use this utility.
            **
            ** parameters: args (string[]). form:
            **   args[0]: ignored
            **   args[1]: operation. possible values:
            **      "Backup": backup tables provided
            **      "SelectiveAdditive": restore all tables provided, only adding new items to tables
            **      "SelectiveOverwrite": restore all tables provided, overwriting existing table
            **      "RestoreAll": identical to "SelectiveOverwrite"
            **   args[2]: database connection string
            **   args[3]: folder in which backups are stored
            **   args[4]: folder of chosen backup to restore. ignored if backing up
            **   args[n>4]: all items in the array with index 5 or greater is taken as list of table names to operate on
            **   
            ** return value: bool[]
            **    this method returns a list of booleans corresponding to whether the operation on each table was successful.
            **    for example, if args = {"", "connection", "C:/backups", "C:/backups/2021-02-24 13.33", "table1", "table2", "table3"} returned
            **    {true, false, true} then the operation was successful on table1 and table3 and failed for table2
            */

            // initialise backup object containing data for this operation
            SQLBackup backup = new SQLBackup(args[1], args[2], args[3], args[4]);
            // success bools corresponding to the tables provided
            bool[] success = new bool[args.Length - 5];
            // true if everything succeeded, false otherwise
            bool all_success = true;

            // for each table...
            for (int i = 5; i < args.Length; i++)
            {
                if (backup.operation == "restore")
                {
                    Console.WriteLine("Restoring table {0} (overwrite={1})", args[i], backup.overwrite);
                    // restore table
                    success[i - 5] = backup.restore_table(args[i]);
                    // sets all_success to false if operation failed, keeps previous value if succeeded
                    all_success = all_success && success[i - 5];
                }
                else
                {
                    Console.WriteLine("Backing up table {0}", args[i]);
                    // backup this table
                    success[i - 5] = backup.backup_table(args[i]);
                    // sets all_success to false if operation failed, keeps previous value if succeeded
                    all_success = all_success && success[i - 5];
                }
            }

            // if backup was successful on all tables, remove backups older than {this.keep_backups_count} backups
            if ((backup.operation == "backup") && all_success)
            {
                // get all folders in the backup folder
                string[] dirs = Directory.GetDirectories(backup.backup_location, "*", SearchOption.TopDirectoryOnly);
                // sort folders in reverse alphabetical order.
                // because each backup is named for the date and time with year first, this is equivalent to sorting backups by date order with newest first
                Array.Sort(dirs);
                Array.Reverse(dirs);
                // remove folders that aren't backups (i.e. don't have a name that is a date in `YYYY-mm-dd hh.mm` format)
                string[] filtered_dirs = Array.FindAll(dirs, x => Regex.Match(x, "[0-9]{4}(-[0-9]{1,2}){2} [0-9]{1,2}\\.[0-9]{1,2}").Success);

                // skip(=keep) first {keep_backups_count} backups and remove all those older than this number of backups ago
                for (int i = backup.keep_backups_count; i < filtered_dirs.Length; i++)
                {
                    Directory.Delete(filtered_dirs[i], true);
                }
            }

            return success;
        }
        private SQLBackup(string op_type, string connection_string, string backup_dir, string active_folder)
        {
            /*
            ** constructor for object that stores all the information provided to us by the user
            */

            this.connection_string = connection_string;

            // convert path to use forward slashes like the rest of the known universe
            this.backup_location = backup_dir.Replace(@"\", @"/");
            if (!this.backup_location.EndsWith("/"))
            {
                // always want a slash at the end for easy file name appending
                this.backup_location += '/';
            }
            // create this folder if it doesn't already exist
            Directory.CreateDirectory(this.backup_location);

            // convert path to use forward slashes like the rest of the known universe
            this.active_folder = active_folder.Replace(@"\", @"/");
            if (!this.active_folder.EndsWith("/"))
            {
                // always want a slash at the end for easy file name appending
                this.active_folder += '/';
            }

            // convert specified operation into useful object properties
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

            // if user has chosen to back up, create a new backup folder with the current date and time as folder name
            if (this.operation == "backup")
            {
                DateTime date = DateTime.Now;
                string datestr = String.Format("{0}-{1}-{2} {3}.{4}",
                    date.Year,
                    date.Month,
                    date.Day,
                    date.Hour,
                    date.Minute
                );
                // set this new folder as active backup folder
                this.active_folder = String.Format("{0}{1}/", this.backup_location, datestr);
            }
            // create this folder if it doesn't already exist
            Directory.CreateDirectory(this.active_folder);
        }
        private string insert_statement(string schema, string table_name, DataRow data, DataColumnCollection cols)
        {
            /*
            ** generates insert statement for a single DataRow of a DataTable
            */
            string[] columns = new string[cols.Count];
            string[] values = new string[cols.Count];

            for (int i = 0; i < cols.Count; i++)
            {
                // add column of table to our convenience array
                columns[i] = cols[i].ColumnName.ToString();
                // add value corresponding to this column to convenience array, respecting NULL values
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

            // generate SQL
            string output = String.Format("INSERT INTO [{0}].[{1}] (\n  ", schema, table_name);
            // String.Join() turns our convenient arrays into a string made up of all the values separated by commas
            output += String.Join(", ", columns);
            output += "\n) VALUES (\n  ";
            output += String.Join(", ", values);
            output += "\n);";

            return output;
        }
        private string insert_where_not_exists(string schema, string table_name, DataRow data, DataColumnCollection cols, string pk_col)
        {
            /*
            ** create an insert statement for database row that only inserts the value if there isn't already an item in the DB with the same ID
            */
            string[] columns = new string[cols.Count];
            string[] values = new string[cols.Count];
            // this is the ID that we should check existence on
            string pk_value = data[pk_col].ToString();

            for (int i = 0; i < cols.Count; i++)
            {
                // add column of table to our convenience array
                columns[i] = cols[i].ColumnName.ToString();
                // add value corresponding to this column to convenience array, respecting NULL values
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

            // generate SQL
            string output = "IF NOT EXISTS (\n";
            // subquery that checks if this value already exists
            output += String.Format("  SELECT * FROM [{0}].[{1}]\n", schema, table_name);
            output += String.Format("  WHERE [{0}] = '{1}'\n", pk_col, pk_value);
            output += ")\nBEGIN\n";
            // normal insert statement to execute if nothing found matching this row
            output += String.Format("  INSERT INTO [{0}].[{1}] (\n  ", schema, table_name);
            // String.Join() turns our convenient arrays into a string made up of all the values separated by commas
            output += String.Join(", ", columns);
            output += "\n  ) VALUES (\n    ";
            output += String.Join(", ", values);
            output += "\n  )\nEND";

            return output;
        }
        private bool backup_table(string table_name)
        {
            /*
            ** creates a backup for a table
            */
            try
            {
                // generates helper object with useful data for this table
                SQLTable table_info = new SQLTable(active_folder, table_name);

                // connect to the database
                SqlConnection conn = new SqlConnection(connection_string);
                conn.Open();
                // read all rows in the database
                SqlCommand command = new SqlCommand("SELECT * FROM " + table_name + ";", conn);
                // dump rows into a DataTable object
                DataTable data = new DataTable();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    data.Load(reader);
                }

                // SQL to get information about the columns in the table
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
                // gets the schema the table belongs to
                string schema = col_data.Rows[0]["TABLE_SCHEMA"].ToString();

                // SQL to get information about all foreign keys in the table
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

                // SQL to get the primary key(s) of the table
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

                // SQL to get unique constraints on columns of the table
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
                    // SQl to delete the table
                    outputFile.WriteLine("-- Remove existing table");
                    outputFile.WriteLine("DROP TABLE IF EXISTS [{0}].[{1}];", schema, table_name);
                    outputFile.WriteLine();

                    // SQL to recreate table from information obtained above
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
                        // add foreign keys
                        outputFile.WriteLine();
                        outputFile.WriteLine("ALTER TABLE [{0}].[{1}]", schema, table_name);
                        outputFile.WriteLine("    ADD CONSTRAINT [{0}]", fk["CONSTRAINT_NAME"]);
                        outputFile.WriteLine("    FOREIGN KEY ([{0}]) REFERENCES [{1}].[{2}] ([{3}]);",
                            fk["COLUMN_NAME"], schema, fk["REFERENCES"], fk["REFERENCES_COLUMN"]
                        );
                    }

                    // now we've remade the table as it was, add rows back into the table
                    outputFile.WriteLine();
                    outputFile.WriteLine("-- Add rows");

                    foreach (DataRow row in data.Rows)
                    {
                        // helper method generates insert statement for this row
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
                        // check if there is a primary key to filter by
                        if (pk_data.Rows.Count > 0)
                        {
                            // helper method generates insert statement for this row
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
                            // use normal insert statement
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
                // handles errors if they arise
                Console.WriteLine(e.Message);
                return false;
            }
        }
        private int find_item(DataRowCollection rows, string col_name, string val)
        {
            /*
            ** finds a DataRow in a DataRowCollection that has a given value for a given column.
            ** returns the index of the DataRow if found, otherwise returns -1
            */
            for (int i = 0; i < rows.Count; i++)
            {
                if (rows[i][col_name].ToString() == val)
                {
                    return i;
                }
            }
            // -1 = not found
            return -1;
        }
        private bool restore_table(string table_name)
        {
            /*
            ** restores a table
            */
            try
            {
                // generates helper object with useful data for this table
                SQLTable table_info = new SQLTable(active_folder, table_name);
                // check if a backup exists
                if (!table_info.has_backup)
                {
                    // no backup to restore from, report failure
                    return false;
                }
                // choose filename to read from depending on the operation chosen
                string filename = overwrite ? table_info.overwrite_file_path : table_info.additive_file_path;

                // open connection to file
                FileInfo file = new FileInfo(filename);
                // read whole SQL script
                string script = file.OpenText().ReadToEnd();
                // connect to the database
                SqlConnection conn = new SqlConnection(connection_string);
                conn.Open();
                // read SQL script into a new command
                SqlCommand command = new SqlCommand(script, conn);
                // run script
                command.ExecuteNonQuery();
                // done
                conn.Close();

                return true;
            }
            catch (Exception e)
            {
                // handle errors
                Console.WriteLine(e.Message);
                return false;
            }
        }
    }
    class SQLTable
    {
        /*
        ** mini helper class used to generate and store some variables relating to a table
        */

        // name of table
        public readonly string name;
        // does this table have a backup to restore from?
        public readonly bool has_backup;
        // file containing overwrite restore script
        public readonly string overwrite_file_path;
        // file containing additive restore script
        public readonly string additive_file_path;

        public SQLTable(string backup_dir, string name)
        {
            this.name = name;
            this.overwrite_file_path = backup_dir + name + "_RestoreOverwrite.sql";
            this.additive_file_path = backup_dir + name + "_RestoreAdditive.sql";
            // check if both scripts exists (i.e. backup exists)
            this.has_backup = File.Exists(this.additive_file_path) && File.Exists(this.overwrite_file_path);
        }
    }
}

