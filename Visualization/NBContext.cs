using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.ProgramSynthesis.Visualization {
    public static class MyExtension
    {
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null)
            {
                return true;
            }
            /* If this is a list, use the Count property for efficiency. 
             * The Count property is O(1) while IEnumerable.Count() is O(N). */
            var collection = enumerable as ICollection<T>;
            if (collection != null)
            {
                return collection.Count < 1;
            }
            return !enumerable.Any();
        }
    }
    /// <summary>
    ///     The class to represent the Notebook Context.
    ///     Currently only includes column names of one data_frame and other single frame names.
    ///     TODO: Support column names list for each data frame. 
    /// </summary>
    public class NbContext {
        /// <summary>
        ///     Creates a <see cref="NbContext"/> using data frame name and column names.
        /// </summary>
        /// <param name="dataFrameName"> Frame name </param>
        /// <param name="columnNames"> Column names </param>
        public NbContext(string dataFrameName, IEnumerable<string> columnNames) {
            DataFrameName = dataFrameName;
            RawColumnNames = columnNames.ToList();
            ColumnNames = RawColumnNames.Select(c => c.ToLower()).ToList();
            RawSingleFrameNames = new List<string>();
            SingleFrameNames = new List<string>();
        }

        /// <summary>
        ///     Creates a <see cref="NbContext"/> using data frame name, column names and column types.
        /// </summary>
        /// <param name="dataFrameName"> Frame name </param>
        /// <param name="columnNames"> Column names </param>
        /// <param name="columnTypes"> Column Types </param>
        /// <param name="columnMins"> The minimums of each column </param>
        /// <param name="columnMaxes"> The maximums of each column </param>
        public NbContext(string dataFrameName, IEnumerable<string> columnNames, IEnumerable<string> columnTypes,
                         IEnumerable<string> columnMins, IEnumerable<string> columnMaxes) {
            DataFrameName = dataFrameName;
            RawColumnNames = columnNames.ToList();
            ColumnNames = RawColumnNames.Select(c => c.ToLower()).ToList();
            RawSingleFrameNames = new List<string>();
            SingleFrameNames = new List<string>();
            ColumnTypes = columnTypes.ToList();
            var columnRanges = new List<Tuple<double, double>>();
            for (int i = 0; i < ColumnTypes.Count; i++) {
                if (ColumnTypes[i].StartsWith("int") || ColumnTypes[i].StartsWith("float")) {
                    if (double.TryParse(columnMins.ElementAt(i), out double l) &&
                        double.TryParse(columnMaxes.ElementAt(i), out double r)) {
                        columnRanges.Add(Tuple.Create(l, r));
                        continue;
                    }
                }

                columnRanges.Add(new Tuple<double, double>(0, 0));
            }

            ColumnRange = columnRanges;
        }

        /// <summary>
        ///     Creates a <see cref="NbContext"/> using data frame name, column names and single column names. 
        /// </summary>
        /// <param name="dataFrameName"> Frame name </param>
        /// <param name="columnNames"> Column names </param>
        /// <param name="singleColumnNames"> Name of the column that is not belong to any data frame. </param>
        public NbContext(string dataFrameName, IEnumerable<string> columnNames, IEnumerable<string> singleColumnNames) {
            DataFrameName = dataFrameName;
            RawColumnNames = columnNames.ToList();
            ColumnNames = RawColumnNames.Select(c => c.ToLower()).ToList();
            RawSingleFrameNames = singleColumnNames.ToList();
            SingleFrameNames = RawSingleFrameNames.Select(c => c.ToLower()).ToList();
        }

        public bool IsCategoricalCol(string s) {
            int? k = RawColumnNames.IndexOf(s);
            if (k.HasValue) return !ColumnTypes.IsNullOrEmpty() && ColumnTypes[k.Value] == "category";
            return false;
        }

        public bool IsPanCategoricalCol(string s) {
            if (IsCategoricalCol(s)) return true;
            int? k = RawColumnNames.IndexOf(s);
            if (k.HasValue) {
                return !ColumnTypes.IsNullOrEmpty() && ColumnTypes[k.Value].Contains("int") && ColumnRange[k.Value].Item2 - ColumnRange[k.Value].Item1 <= 12;
            }
            return false;
        }

        public double GetMin(string s) {
            int? k = RawColumnNames.IndexOf(s);
            if (k.HasValue) return ColumnRange[k.Value].Item1;
            return 0;
        }

        public double GetMax(string s) {
            int? k = RawColumnNames.IndexOf(s);
            if (k.HasValue) return ColumnRange[k.Value].Item2;
            return 0;
        }

        /// <summary>
        ///     Column names that change to lowercase to be easier matching with the tokens.
        /// </summary>
        public IReadOnlyList<string> ColumnNames { get; }

        /// <summary>
        ///     Data frame name.
        /// </summary>
        public string DataFrameName { get; }

        /// <summary>
        ///     Original column names used to synthesize the call.
        /// </summary>
        public List<string> RawColumnNames { get; }

        /// <summary>
        ///     Original single column names.
        /// </summary>
        public IReadOnlyList<string> RawSingleFrameNames { get; }

        /// <summary>
        ///     Single column names that change to lowercase.
        /// </summary>
        public List<string> SingleFrameNames { get; }

        /// <summary>
        ///     Column type information.
        /// </summary>
        public IReadOnlyList<string> ColumnTypes { get; }

        /// <summary>
        ///     Column range information.
        /// </summary>
        public IReadOnlyList<Tuple<double, double>> ColumnRange { get;  }
    }
}