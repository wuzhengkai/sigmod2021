using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.ProgramSynthesis.Visualization.Grammar;

namespace Microsoft.ProgramSynthesis.Visualization {
    /// <summary>
    ///     The class that represents the parsing result of a certain interval.
    /// </summary>
    [DebuggerDisplay("Semantics: {DebuggerDisplay}")]
    public class Parse {
        internal const string Value = "Value";
        private string _tempColumnName = "";
        private NbContext _context;

        /// <summary>
        ///     Creates a <see cref="Parse"/>.
        /// </summary>
        /// <param name="rule"> The rule used. </param>
        /// <param name="children"> Children in the parse tree. </param>
        /// <param name="semantics"> Semantic properties. </param>
        public Parse(Rule rule, IReadOnlyList<Parse> children, IDictionary<string, string> semantics) {
            Rule = rule;
            Children = children;
            Semantics = semantics;
            FitScore = 1.0;
        }

        /// <summary>
        ///     Creates a <see cref="Parse"/> using a given fitScore.
        /// </summary>
        /// <param name="rule"> The rule used. </param>
        /// <param name="children"> Children in the parse tree. </param>
        /// <param name="semantics"> Semantic properties. </param>
        /// <param name="score"> fitScore </param>
        public Parse(Rule rule, IReadOnlyList<Parse> children, IDictionary<string, string> semantics, double score) {
            Rule = rule;
            Children = children;
            Semantics = semantics;
            FitScore = score;
        }

        /// <summary>
        ///     The API synthesized by the parsing result of root.
        ///     Should only be a value other than #N/A when the parse result is root and on the whole interval.
        /// </summary>
        public string Code { get; private set; } = "#N/A";

        /// <summary>
        ///     FitScore of this parse.
        /// </summary>
        public double FitScore { get; }

        /// <summary>
        ///     The rule used in creating the parse.
        /// </summary>
        public Rule Rule { get; }

        /// <summary>
        ///     Children of this parse.
        /// </summary>
        public IReadOnlyList<Parse> Children { get; }

        /// <summary>
        ///     Semantics of this parse, stored in (key, value) style.
        /// </summary>
        public IDictionary<string, string> Semantics { get; }

        private string SemanticsDisplay => string.Join(",", Semantics);

        /// <summary>
        ///     Used for debugging, displaying the results of properties and fitScore.
        /// </summary>
        public string DebuggerDisplay => string.Join(",", Semantics) + $"FitScore = {FitScore}" + $"Lhs = {Rule.Lhs}";

        /// <summary>
        ///     Sets the context of this Parse.
        /// </summary>
        /// <param name="context"></param>
        public void SetContext(NbContext context) => _context = context;

        /// <summary>
        ///     Checks whether two parse results are same.
        /// </summary>
        /// <param name="t"></param>
        /// <returns> True if both lhs symbol and semantics are the same. </returns>
        public bool IsSameParseResult(Parse t) => Rule.Lhs == t.Rule.Lhs && SemanticsDisplay == t.SemanticsDisplay;

        private void SetError() => Code = "#Error";

        private void SetUnsupported() => Code = "#Unsupported";

        private string GetRawColumnName(string s) {
            int? k = _context.RawColumnNames.IndexOf(s);
            if (k.HasValue) return _context.RawColumnNames[k.Value];
            return null;
        }

        private string GetColumnName(string s) {
            int? k = _context.RawColumnNames.IndexOf(s);
            if (k.HasValue)
                return $"{GetCurrentFrameName()}[\"{_context.RawColumnNames[k.Value]}\"]";
            else {
                k = _context.SingleFrameNames.IndexOf(s);
                if (k.HasValue)
                    return _context.RawSingleFrameNames[k.Value];
            }

            return null;
        }

        private string[] GetHistoRawAttrNames(string s) {
            string[] attrs = s.Split(';');
            for (int i = 0; i < attrs.Length; ++i)
                attrs[i] = GetRawColumnName(attrs[i]);
            return attrs.Where(x => !string.IsNullOrEmpty(x)).ToArray();
        }

        private string[] GetHistoAttrNames(string s) {
            string[] attrs = s.Split(';');
            for (int i = 0; i < attrs.Length; ++i)
                attrs[i] = GetColumnName(attrs[i]);
            return attrs.Where(x => !string.IsNullOrEmpty(x)).ToArray();
        }

        private static bool ContainDateElement(string s) {
            return s.Contains("year") || s.Contains("month") || s.Contains("day") || s.Contains("time");
        }

        private bool HasCategorialCol(string s) {
            string[] attrs = s.Split(';');
            for (int i = 0; i < attrs.Length; ++i)
                attrs[i] = GetRawColumnName(attrs[i]);
            var nonnull = attrs.Where(x => !string.IsNullOrEmpty(x)).ToArray();
            int sc = -1; string res = "";
            foreach (string s1 in nonnull) {
                int tmp = 0;
                if (_context.IsPanCategoricalCol(s1)) tmp += 5;
                if (ContainDateElement(s1)) tmp += 2;
                if (tmp > sc) {
                    sc = tmp;
                    res = s1;
                }
            }
            return sc > 0;
        } 

        private string PossibleXaxis(string s) {
            string[] attrs = s.Split(';');
            for (int i = 0; i < attrs.Length; ++i)
                attrs[i] = GetRawColumnName(attrs[i]);
            var nonnull = attrs.Where(x => !string.IsNullOrEmpty(x)).ToArray();
            int sc = -1; string res = "";
            foreach (string s1 in nonnull) {
                int tmp = 0;
                if (_context.IsPanCategoricalCol(s1)) tmp += 5;
                if (ContainDateElement(s1)) tmp += 2;
                if (tmp > sc) {
                    sc = tmp;
                    res = s1;
                }
            }
            return res;
        }

        private string PossibleYaxis(string s) {
            string[] attrs = s.Split(';');
            for (int i = 0; i < attrs.Length; ++i)
                attrs[i] = GetRawColumnName(attrs[i]);
            var nonnull = attrs.Where(x => !string.IsNullOrEmpty(x)).ToArray();
            int sc = 10; string res = "";
            foreach (string s1 in nonnull) {
                int tmp = 0;
                if (_context.IsPanCategoricalCol(s1)) tmp += 5;
                if (ContainDateElement(s1)) tmp += 2;
                if (tmp <= sc) {
                    sc = tmp;
                    res = s1;
                }
            }
            return res;
        }

        private string GetScatterAttrNames(string s) {
            string[] attrs = s.Split(';');
            for (int i = 0; i < attrs.Length; ++i)
                attrs[i] = GetColumnName(attrs[i]);
            var nonnull = attrs.Where(x => !string.IsNullOrEmpty(x)).ToArray();
            if (nonnull.Length < 2) return "Error";
            else return $"x={GetColumnName(PossibleXaxis(s))}, y={GetColumnName(PossibleYaxis(s))}";
        }

        private string GetLineAttrNames(string s) {
            string[] attrs = s.Split(';');
            for (int i = 0; i < attrs.Length; ++i)
                attrs[i] = GetColumnName(attrs[i]);
            var nonnull = attrs.Where(x => !string.IsNullOrEmpty(x)).ToArray();
            if (nonnull.Length < 1) return "Error";
            else if (nonnull.Length == 1) return $"{nonnull[0]}";
            else return $"{GetColumnName(PossibleXaxis(s))}, {GetColumnName(PossibleYaxis(s))}";
        }

        private string GetCurrentFrameName() {
            if (!String.IsNullOrEmpty(_tempColumnName)) return _tempColumnName;
            return _context.DataFrameName;
        }

        private string GetAxisAndLegends(string type, string xaxis, string yaxis, string series, string aggr) {
            Code = "";
            if (!string.IsNullOrEmpty(xaxis)) Code += $"plt.xlabel(\'{xaxis}\')\n";
            if (!string.IsNullOrEmpty(yaxis)) Code += $"plt.ylabel(\'{aggr}{yaxis}\')\n";
            if (!string.IsNullOrEmpty(series)) Code += $"plt.legend(title=\'{series}\')\n";
            return Code;
        }

        private string GetLabel(string label) {
            if (!string.IsNullOrEmpty(label)) return $", label={label}";
            return "";
        }

        private string AppendString(string cur, string now) {
            if (string.IsNullOrEmpty(cur)) return now;
            return cur + "; " + now;
        }

        private string RemoveString(string cur, string now) {
            return String.Join(";",cur.Split(new char[] {';'}).Where(x => x!=now));
        }

        private void SetProfiling() {
            Code = $"{GetCurrentFrameName()}.profile_report()";
        }

        private double GetMinOfColumn(string s) => _context.GetMin(s);

        private double GetMaxOfColumn(string s) => _context.GetMax(s);

        private bool HasMultipleProperty(string s) {
            return Semantics.ContainsKey(s) && Semantics[s].Contains(";");
        }

        private bool SynthesizeRealPlotAPI(string label, string padding, string series, string aggr, bool _enablelegends) {
            if (Semantics.ContainsKey("type") && Semantics["type"] == "histogram") {
                // Synthesize plt.hist
                if (!Semantics.ContainsKey("ColName")) {
                    SetError();
                    return true;
                }

                string[] attrs = GetHistoAttrNames(Semantics["ColName"]);
                string[] rawattrs = GetHistoRawAttrNames(Semantics["ColName"]);
                string alpha = "";

                if (!string.IsNullOrEmpty(label)) alpha = ", alpha=0.5";

                string bins = "";
                if (Semantics.ContainsKey("histobins")) {
                    if (Semantics.ContainsKey("bin_pair_first") && Semantics.ContainsKey("bin_pair_second")) {
                        if (double.TryParse(Semantics["bin_pair_first"], out double first) &&
                            double.TryParse(Semantics["bin_pair_second"], out double second)) {
                            if (first > second) {
                                double mid = first;
                                first = second;
                                second = mid;
                            }

                            if (Semantics.ContainsKey("bin_pair_step") &&
                                double.TryParse(Semantics["bin_pair_step"], out double step)) {
                                if (step < 0) step = -step;
                                if (step > second) {
                                    double mid = step;
                                    step = second;
                                    second = mid;
                                }

                                bins = $", bins=np.arange({first}, {second + 0.5}, {step})";
                            }
                            else {
                                if (Semantics.ContainsKey("bin_single") &&
                                    double.TryParse(Semantics["bin_single"], out double step1)) {
                                    if (step1 < 0) step1 = -step1;
                                    if (step1 > second) {
                                        double mid = step1;
                                        step = second;
                                        second = mid;
                                    }

                                    bins = $", bins=np.arange({first}, {second + 0.5}, {step1})";
                                }
                                else bins = $", range=[{first}, {second}]";
                            }
                        }
                    }
                    else {
                        if (Semantics.ContainsKey("bin_single")) {
                            string value = Semantics["bin_single"];
                            if (!value.Contains(',')) {
                                if (int.TryParse(value, out int tmp)) {
                                    bins = ", bins=" + tmp;
                                }
                            }
                            else {
                                bins = ", bins=[" + value + "]";
                            }
                        }
                        else {
                            if (Semantics.ContainsKey("bin_pair_step")) {
                                if (double.TryParse(Semantics["bin_pair_step"], out double step)) {
                                    double first = GetMinOfColumn(rawattrs[0]);
                                    double second = GetMaxOfColumn(rawattrs[0]);
                                    bins = $", bins=np.arange({first}, {second + 0.5}, {step})";
                                }
                            }
                        }
                    }
                }

                string options = "";
                if (Semantics.ContainsKey("histolog")) options += ", log=True";
                if (Semantics.ContainsKey("histostack")) options += ", stacked=True";
                if (Semantics.ContainsKey("histodens")) options += ", density=True";

                string yaxis = "Frequency";
                if (Semantics.ContainsKey("histolog")) yaxis = "Logarithmic " + yaxis;
                if (Semantics.ContainsKey("histodens")) yaxis = "Normalized " + yaxis;

                if (attrs.Length > 1) {
                    string attr = "[" + string.Join(",", attrs) + "]";
                    string lbl = "[" + string.Join(",", rawattrs.Select(x => "\'" + AppendString(label, x) + "\'")) + "]";
                    Code += $"{padding}plt.hist({attr}{bins}{options}{alpha}{GetLabel(lbl)})";
                    if (_enablelegends)
                        Code += "\n" + GetAxisAndLegends("histogram", "", yaxis, AppendString(series, "column"), aggr);
                }
                else {
                    Code += string.Join("\n", attrs.Select(x => $"{padding}plt.hist({x}{bins}{options}{alpha}{GetLabel(label)})"));
                    if (_enablelegends)
                        Code += "\n" + GetAxisAndLegends("histogram", rawattrs[0], yaxis, series, aggr);
                }
                return true;
            }

            if (Semantics.ContainsKey("regress")) {
                // synthesize sns.lmplot
                if (!Semantics.ContainsKey("ColName")) {
                    SetError();
                    return true;
                }

                Code += $"{padding}sns.lmplot(x=\'{PossibleXaxis(Semantics["ColName"])}\'," +
                        $"y=\'{PossibleYaxis(Semantics["ColName"])}\', data={GetCurrentFrameName()})\n";

                return true;
            }

            if (Semantics.ContainsKey("type") && Semantics["type"] == "scatter") {
                // synthesize plt.scatter
                if (!Semantics.ContainsKey("ColName")) {
                    SetError();
                    return true;
                }

                string attrs = GetScatterAttrNames(Semantics["ColName"]);
                if (attrs == "Error") {
                    SetError();
                    return true;
                }

                string options = "";

                if (Semantics.ContainsKey("marker") && double.TryParse(Semantics["marker"], out double size)) {
                    options += $", s={Semantics["marker"]}";
                }
                else if (Semantics.ContainsKey("marker_col")) {
                    string tmp = GetColumnName(Semantics["marker_col"]);
                    if (!string.IsNullOrEmpty(tmp)) options += $", s={tmp}";
                }

                if (Semantics.ContainsKey("color")) {
                    options += $", c='{Semantics["color"][0]}'";
                }
                else if (Semantics.ContainsKey("color_col")) {
                    string tmp = GetColumnName(Semantics["color_col"]);
                    if (string.IsNullOrEmpty(tmp)) options += $", c={tmp}";
                }

                string xaxis = PossibleXaxis(Semantics["ColName"]);
                string yaxis = PossibleYaxis(Semantics["ColName"]);
                Code += $"{padding}plt.scatter({attrs}{options}{GetLabel(label)})";
                if (_enablelegends)
                    Code += "\n" + GetAxisAndLegends("scatter", xaxis, yaxis, series, aggr);
                return true;
            }

            if (Semantics.ContainsKey("type") && Semantics["type"] == "lineplot") {
                // synthesize plt.plot
                if (!Semantics.ContainsKey("ColName")) {
                    SetError();
                    return true;
                }

                string attrs = GetLineAttrNames(Semantics["ColName"]);
                if (attrs == "Error") {
                    SetError();
                    return true;
                }

                string options = "";
                if (Semantics.ContainsKey("marker")) options += Semantics["marker"];
                if (Semantics.ContainsKey("line")) options += Semantics["line"];
                if (Semantics.ContainsKey("color")) options += Semantics["color"];

                if (options != "") options = $", \'{options}\'";

                string xaxis = "";
                string yaxis = PossibleYaxis(Semantics["ColName"]);
                if (HasMultipleProperty("ColName")) xaxis = PossibleXaxis(Semantics["ColName"]);
                Code += $"{padding}plt.plot({attrs}{options}{GetLabel(label)})";
                if (_enablelegends)
                    Code += "\n" + GetAxisAndLegends("line", xaxis, yaxis, series, aggr);
                return true;
            }

            return false;
        }

        /// <summary>
        ///     The method that synthesize the final Matplotlib API call.
        ///     The result is set in <see cref="Parse.Code"/>.
        /// </summary>
        public void SynthesizeCall() {
            if (!Semantics.ContainsKey("type")) {
                if (!Semantics.ContainsKey("ColName")) {
                    SetProfiling();
                    return;
                }
            }

            if (Rule.Lhs.Name != Grammar.Grammar.Root) return;
            Code = "";

            if (!Semantics.ContainsKey("type")) {
                if (!Semantics.ContainsKey("ColName")) {
                    SetError();
                    return;
                }
                int hist = 0, scatter = 0, line = 0;
                int p = Semantics["ColName"].Count(x => x == ';') + 1;
                int q = 0;
                if (Semantics.ContainsKey("group_col")) q = Semantics["group_col"].Count(x => x == ';') + 1;
                if (p == 2 || p == 1 && q > 1) {
                    scatter++;
                    line++;
                }
                else {
                    scatter -= 2;
                    line -= 2;
                }

                if (Semantics.ContainsKey("histobins")) hist++;
                if (Semantics.ContainsKey("histostack")) hist++;
                if (Semantics.ContainsKey("histolog")) hist++;
                if (Semantics.ContainsKey("histodens")) hist++;
                if (Semantics.ContainsKey("marker") || Semantics.ContainsKey("marker_col")) scatter++;
                if (Semantics.ContainsKey("color") || Semantics.ContainsKey("color_col")) scatter++;
                if (Semantics.ContainsKey("marker")) line++;
                if (Semantics.ContainsKey("line")) line++;
                if (Semantics.ContainsKey("color")) line++;
                if (_context.IsPanCategoricalCol(PossibleXaxis(Semantics["ColName"]))) scatter++;
                else line++;

                int maxi = new int[] { hist, scatter, line }.Max();
                if (scatter == maxi) Semantics["type"] = "scatter";
                if (line == maxi) Semantics["type"] = "lineplot";
                if (hist == maxi) Semantics["type"] = "histogram";
            }

            string label = "";
            string padding = "";
            string series = "";
            string aggr = "";

            if (Semantics.ContainsKey("group_op")) {
                if (!Semantics.ContainsKey("group_col")) {
                    if (_context.IsPanCategoricalCol(PossibleXaxis(Semantics["ColName"]))
                        && (Semantics["type"] == "lineplot" || Semantics["type"] == "scatter"))
                        Semantics["group_col"] = PossibleXaxis(Semantics["ColName"]);
                }
            }

            if (Semantics.ContainsKey("group_col")) {
                if (Semantics.ContainsKey("ColName")) {
                    {
                        string p1 = PossibleXaxis(Semantics["ColName"]);
                        string p2 = PossibleXaxis(Semantics["group_col"]);
                        //Console.WriteLine(p1 + " " + p2);
                        if (_context.IsPanCategoricalCol(p1) && !_context.IsPanCategoricalCol(p2)) {
                            Semantics["ColName"] = AppendString(RemoveString(Semantics["ColName"], p1), p2);
                            Semantics["group_col"] = AppendString(RemoveString(Semantics["group_col"], p2), p1);
                            //Console.WriteLine(Semantics["ColName"] + " " + Semantics["group_col"]);
                        }
                    }

                    string grpcol =
                        String.Join(", ", Semantics["group_col"].Split(';').Select(x => $"\'{x}\'"));
                    string grpcol1 = grpcol;
                    bool flag = false;
                    if (!(Semantics.ContainsKey("type") && Semantics["type"] == "histogram")) {
                        if (HasMultipleProperty("ColName") && HasCategorialCol(Semantics["ColName"])) {
                            if (PossibleXaxis(Semantics["ColName"]) == Semantics["group_col"].Split(';')[0])
                                flag = true;
                            else grpcol1 = grpcol + ",\'" + PossibleXaxis(Semantics["ColName"]) + "\'";
                        }
                    }

                    if (!Semantics.ContainsKey("group_op") &&
                        !_context.IsPanCategoricalCol(PossibleYaxis(Semantics["ColName"])) &&
                        !(Semantics.ContainsKey("type") && Semantics["type"] == "histogram")) {
                        Semantics["group_op"] = "mean";
                    }

                    if (Semantics.ContainsKey("group_op")) aggr = Semantics["group_op"] + " ";

                    string col = Semantics["ColName"];
                    if ((!col.Contains(";") || flag) && !(Semantics.ContainsKey("type") && Semantics["type"] == "histogram") &&
                        !HasMultipleProperty("group_col")) {
                        if (!flag)
                            col = Semantics["group_col"].Split(';')[0] + ";" + col;
                        Semantics["ColName"] = col;

                        if (Semantics.ContainsKey("group_op")) {
                            Code += $"df_tt = {GetCurrentFrameName()}.groupby([{grpcol}]).{Semantics["group_op"]}().reset_index()\n";
                            _tempColumnName = "df_tt";
                        }
                    } else {
                        padding = "    ";
                        label = "str(cc)";
                        series = String.Join(", ", Semantics["group_col"].Split(';'));

                        if (!col.Contains(";") &&
                            !(Semantics.ContainsKey("type") && Semantics["type"] == "histogram")) {
                            col = Semantics["group_col"].Split(';')[0] + ";" + col;
                            grpcol = String.Join(", ", Semantics["group_col"].Split(';').Skip(1).Select(x => $"\'{x}\'"));
                            series = String.Join(", ", Semantics["group_col"].Split(';').Skip(1));
                            Semantics["ColName"] = col;
                        }

                        if (Semantics.ContainsKey("group_op"))
                            Code +=
                                $"df_tt = {GetCurrentFrameName()}.groupby([{grpcol1}]).{Semantics["group_op"]}().groupby([{grpcol}])\n";
                        else
                            Code +=
                                $"df_tt = {GetCurrentFrameName()}.groupby([{grpcol}])\n";

                        Code += $"for cc, df_tt1 in df_tt:\n{padding}df_tt2 = df_tt1.reset_index()\n";
                        _tempColumnName = "df_tt2";
                    }
                }
            }

            if (Semantics.ContainsKey("filter_col")) {
                if (Semantics.ContainsKey("filter_v") && Semantics.ContainsKey("filter_op")) {
                    if (!HasMultipleProperty("filter_v")) {
                        string col = GetColumnName(Semantics["filter_col"]);
                        series = AppendString(series, GetRawColumnName(Semantics["filter_col"]));
                        if (Semantics["filter_op"] != "==")
                            label = AppendString(label, $"\'{Semantics["filter_op"]} {Semantics["filter_v"]}\'");
                        else label = AppendString(label, $"\'{Semantics["filter_v"]}\'");
                        //Random rnd = new Random();
                        //TempColumnName = "df" + rnd.Next(1000).ToString("D3");
                        Code +=
                            $"{padding}df_t = {GetCurrentFrameName()}.loc[{col} {Semantics["filter_op"]} {Semantics["filter_v"]}]\n";
                        _tempColumnName = "df_t";
                    }
                    else {
                        string[] filterval = Semantics["filter_v"].Split(';');
                        string[] filterop = Semantics["filter_op"].Split(';');
                        string tmps = series, tmpl = label;
                        for (int i = 0; i < filterval.Length; ++i) {
                            string col = GetColumnName(Semantics["filter_col"]);
                            series = AppendString(tmps, GetRawColumnName(Semantics["filter_col"]));
                            if (filterop[i] != "==")
                                label = AppendString(tmpl, $"\'{filterop[i]} {filterval[i]}\'");
                            else label = AppendString(tmpl, $"\'{filterval[i]}\'");
                            Code +=
                                $"{padding}df_t{i} = {GetCurrentFrameName()}.loc[{col} {filterop[i]} {filterval[i]}]\n";
                            string tmp = GetCurrentFrameName();
                            _tempColumnName = "df_t" + i.ToString();
                            if (i != filterval.Length - 1) {
                                SynthesizeRealPlotAPI(label, padding, series, aggr, false);
                                Code += "\n";
                            }
                            else {
                                SynthesizeRealPlotAPI(label, padding, series, aggr, true);
                            }
                            _tempColumnName = tmp;
                        }
                        return;
                    }
                }

            }

            if (!SynthesizeRealPlotAPI(label, padding, series, aggr, true))
                SetUnsupported();

            return;
        }
    }
}