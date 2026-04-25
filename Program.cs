// ============================================================
//  Program.cs  —  WASM Browser entry point
//  Propositions-as-Types running in the browser via .NET WASM
//  (no Blazor — pure JSExport / JSImport interop)
// ============================================================

using System.Runtime.InteropServices.JavaScript;
using PATBrowser.Core;

Console.WriteLine("[PAT] .NET WASM runtime started");

// Keep the runtime alive; JavaScript drives everything via JSExport calls.
await Task.Delay(-1);

// ─── Bridge: C# ↔ JavaScript ─────────────────────────────────────────────────

/// <summary>
/// All methods decorated with [JSExport] are callable from JavaScript.
/// All methods decorated with [JSImport] call into JavaScript.
/// </summary>
public static partial class PATBridge
{
    // ── JSImport: call JS from C# ─────────────────────────────────────────────

    [JSImport("dom.setInnerHTML", "main.js")]
    internal static partial void SetInnerHTML(string selector, string html);

    // ── JSExport: call C# from JS ─────────────────────────────────────────────

    /// <summary>Returns the Curry-Howard correspondence table as HTML.</summary>
    [JSExport]
    public static string GetCorrespondenceTableHTML()
    {
        var rows = new[]
        {
            ("Truth",         "⊤",     "TrueProof",                   "new TrueProof()"),
            ("Falsity",       "⊥",     "FalseProof",                  "<em>(uninhabited)</em>"),
            ("Conjunction",   "A ∧ B", "AndProof&lt;A,B&gt;",         "ConjunctionIntro(a, b)"),
            ("Disjunction",   "A ∨ B", "OrProof&lt;A,B&gt; (OneOf)",  "DisjunctionIntroLeft(a)"),
            ("Implication",   "A → B", "ImpliesProof&lt;A,B&gt;",     "ImplicationIntro(f)"),
            ("Negation",      "¬A",    "NotProof&lt;A&gt;",           "NegationIntro(refute)"),
            ("Biconditional", "A ↔ B", "IffProof&lt;A,B&gt;",         "BiconditionalIntro(f, g)"),
        };

        var sb = new System.Text.StringBuilder();
        sb.Append("<table><thead><tr><th>Logic</th><th>Proposition</th><th>C# Type</th><th>Proof Term</th></tr></thead><tbody>");
        foreach (var (logic, prop, type, term) in rows)
            sb.Append($"<tr><td>{logic}</td><td class=\"prop\">{prop}</td><td class=\"code\">{type}</td><td class=\"code\">{term}</td></tr>");
        sb.Append("</tbody></table>");
        return sb.ToString();
    }

    /// <summary>Returns the inference rules table as HTML.</summary>
    [JSExport]
    public static string GetRulesTableHTML()
    {
        var rules = new[]
        {
            ("∧-Intro",  "Conjunction Introduction",    "A, B",              "A ∧ B",    "ConjunctionIntro"),
            ("∧-ElimL",  "Conjunction Elim Left",       "A ∧ B",             "A",         "ConjunctionElimLeft"),
            ("∧-ElimR",  "Conjunction Elim Right",      "A ∧ B",             "B",         "ConjunctionElimRight"),
            ("∨-IntroL", "Disjunction Intro Left",      "A",                 "A ∨ B",    "DisjunctionIntroLeft"),
            ("∨-IntroR", "Disjunction Intro Right",     "B",                 "A ∨ B",    "DisjunctionIntroRight"),
            ("∨-Elim",   "Disjunction Elimination",     "A∨B, A→C, B→C",     "C",         "DisjunctionElim"),
            ("DS",       "Disjunctive Syllogism",        "A∨B, ¬A",           "B",         "DisjunctiveSyllogismLeft"),
            ("CD",       "Constructive Dilemma",         "A∨B, A→C, B→D",     "C∨D",      "ConstructiveDilemma"),
            ("→-Intro",  "Implication Introduction",    "[A] ⊢ B",           "A → B",    "ImplicationIntro"),
            ("MP",       "Modus Ponens",                 "A→B, A",            "B",         "ModusPonens"),
            ("MT",       "Modus Tollens",                "A→B, ¬B",           "¬A",        "ModusTollens"),
            ("HS",       "Hypothetical Syllogism",       "A→B, B→C",          "A→C",      "HypotheticalSyllogism"),
            ("Abs",      "Absorption",                   "A→B",               "A→(A∧B)",  "Absorption"),
            ("¬-Intro",  "Negation Introduction",        "[A] ⊢ ⊥",           "¬A",        "NegationIntro"),
            ("ECQ",      "Ex Contradictione Quodlibet",  "A, ¬A",             "C",         "ExFalso"),
            ("DNE",      "Double Negation Elimination",  "¬¬A",               "A",         "DoubleNegationElim"),
            ("↔-Intro",  "Biconditional Introduction",  "A→B, B→A",          "A↔B",      "BiconditionalIntro"),
            ("↔-ElimL",  "Biconditional Elim Forward",  "A↔B, A",            "B",         "BiconditionalElimForward"),
            ("↔-ElimR",  "Biconditional Elim Backward", "A↔B, B",            "A",         "BiconditionalElimBackward"),
        };

        var sb = new System.Text.StringBuilder();
        sb.Append("<table><thead><tr><th>Symbol</th><th>Rule Name</th><th>Premises</th><th>Conclusion</th><th>C# Method</th></tr></thead><tbody>");
        foreach (var (sym, name, premises, conc, method) in rules)
            sb.Append($"<tr><td class=\"prop\">{sym}</td><td>{name}</td><td class=\"code\">{premises}</td><td class=\"prop\">{conc}</td><td class=\"code\">{method}</td></tr>");
        sb.Append("</tbody></table>");
        return sb.ToString();
    }

    /// <summary>
    /// Runs a named proof demo and returns the proof tree as HTML.
    /// demoName: "mp" | "mt" | "hs" | "conj" | "disj" | "iff" | "abs" | "search"
    /// </summary>
    [JSExport]
    public static string RunDemo(string demoName)
    {
        return demoName switch
        {
            "mp"     => DemoModusPonens(),
            "mt"     => DemoModusTollens(),
            "hs"     => DemoHypotheticalSyllogism(),
            "conj"   => DemoConjunction(),
            "disj"   => DemoDisjunction(),
            "iff"    => DemoBiconditional(),
            "abs"    => DemoAbsorption(),
            "search" => DemoProofSearch(),
            _        => "<p class='error'>Unknown demo.</p>"
        };
    }

    // ── Demo implementations ─────────────────────────────────────────────────

    private static string DemoModusPonens()
    {
        var p = new Atom("P");
        var pImpliesQ = InferenceRules
            .ImplicationIntro<Atom, Atom>(_ => new Atom("Q"), "P → Q")
            .First();
        var node = new DerivedAtom("Q", "MP", new IProofNode[] { pImpliesQ, p });
        return ProofTreeToHTML(node, "Modus Ponens: P, P→Q ⊢ Q");
    }

    private static string DemoModusTollens()
    {
        var pImpliesQ = InferenceRules
            .ImplicationIntro<Atom, Atom>(_ => new Atom("Q"), "P → Q")
            .First();
        var notQ = new NotProof<Atom>(_ => new FalseProof("¬Q applied"), Array.Empty<IProofNode>());
        var node = new DerivedAtom("¬P", "MT", new IProofNode[] { pImpliesQ, notQ });
        return ProofTreeToHTML(node, "Modus Tollens: P→Q, ¬Q ⊢ ¬P");
    }

    private static string DemoHypotheticalSyllogism()
    {
        var pq = InferenceRules.ImplicationIntro<Atom, Atom>(_ => new Atom("Q"), "P → Q").First();
        var qr = InferenceRules.ImplicationIntro<Atom, Atom>(_ => new Atom("R"), "Q → R").First();
        var node = new DerivedAtom("P → R", "HS", new IProofNode[] { pq, qr });
        return ProofTreeToHTML(node, "Hypothetical Syllogism: P→Q, Q→R ⊢ P→R");
    }

    private static string DemoConjunction()
    {
        var p = new Atom("P");
        var q = new Atom("Q");
        var pAndQ = InferenceRules.ConjunctionIntro(p, q).First();
        var elimNode = new DerivedAtom("P", "∧-ElimL", new IProofNode[] { pAndQ });
        return ProofTreeToHTML(pAndQ, "Conjunction Intro: P, Q ⊢ P ∧ Q")
             + ProofTreeToHTML(elimNode, "Conjunction Elim Left: P ∧ Q ⊢ P");
    }

    private static string DemoDisjunction()
    {
        var p = new Atom("P");
        var r = new Atom("R");
        var pOrQ = InferenceRules.DisjunctionIntroLeft<Atom, Atom>(p).First();
        var pToR = InferenceRules.ImplicationIntro<Atom, Atom>(_ => r, "P → R").First();
        var qToR = InferenceRules.ImplicationIntro<Atom, Atom>(_ => r, "Q → R").First();
        var rNode = new DerivedAtom("R", "∨-Elim", new IProofNode[] { pOrQ, pToR, qToR });
        return ProofTreeToHTML(pOrQ, "Disjunction Intro Left (OneOf): P ⊢ P ∨ Q")
             + ProofTreeToHTML(rNode, "Case Analysis: P∨Q, P→R, Q→R ⊢ R");
    }

    private static string DemoBiconditional()
    {
        var p = new Atom("P");
        var q = new Atom("Q");
        var pToQ = InferenceRules.ImplicationIntro<Atom, Atom>(_ => q, "P → Q").First();
        var qToP = InferenceRules.ImplicationIntro<Atom, Atom>(_ => p, "Q → P").First();
        var iff  = InferenceRules.BiconditionalIntro(pToQ, qToP).First();
        return ProofTreeToHTML(iff, "Biconditional Intro: P→Q, Q→P ⊢ P↔Q");
    }

    private static string DemoAbsorption()
    {
        var pq = InferenceRules.ImplicationIntro<Atom, Atom>(_ => new Atom("Q"), "P → Q").First();
        var abs = InferenceRules.Absorption(pq).First();
        return ProofTreeToHTML(abs, "Absorption: P→Q ⊢ P→(P∧Q)");
    }

    private static string DemoProofSearch()
    {
        var ctx = new ProofContext().Assume("A").Assume("B").Assume("C");
        var sb = new System.Text.StringBuilder();

        sb.Append("<h3>All conjunctions derivable from {A, B, C}</h3>");
        sb.Append("<table><thead><tr><th>Conjunction</th><th>Left</th><th>Right</th></tr></thead><tbody>");
        foreach (var conj in ProofSearch.AllConjunctions(ctx))
            sb.Append($"<tr><td class=\"prop\">{conj.Proposition}</td><td>{conj.Left.Proposition}</td><td>{conj.Right.Proposition}</td></tr>");
        sb.Append("</tbody></table>");

        sb.Append("<h3>All disjunctions derivable from {A, B, C} (first 6)</h3>");
        sb.Append("<table><thead><tr><th>Disjunction</th><th>Injection Rule</th></tr></thead><tbody>");
        foreach (var disj in ProofSearch.AllDisjunctions(ctx).Take(6))
            sb.Append($"<tr><td class=\"prop\">{disj.Proposition}</td><td class=\"code\">{disj.RuleName}</td></tr>");
        sb.Append("</tbody></table>");

        return sb.ToString();
    }

    // ── Proof tree → HTML ────────────────────────────────────────────────────

    private static string ProofTreeToHTML(IProofNode node, string title)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append($"<div class=\"proof-block\"><div class=\"proof-title\">{EscapeHtml(title)}</div>");
        sb.Append("<div class=\"proof-tree\">");
        RenderNode(sb, node, "", true);
        sb.Append("</div></div>");
        return sb.ToString();
    }

    private static void RenderNode(System.Text.StringBuilder sb, IProofNode node, string prefix, bool isLast)
    {
        var connector = isLast ? "└── " : "├── ";
        sb.Append($"<div class=\"tree-line\">" +
                  $"<span class=\"tree-prefix\">{EscapeHtml(prefix + connector)}</span>" +
                  $"<span class=\"rule-tag\">{EscapeHtml(node.RuleName)}</span>" +
                  $" ⊢ " +
                  $"<span class=\"prop-label\">{EscapeHtml(node.Proposition)}</span>" +
                  $"</div>");

        var childPrefix = prefix + (isLast ? "    " : "│   ");
        for (int i = 0; i < node.Premises.Count; i++)
            RenderNode(sb, node.Premises[i], childPrefix, i == node.Premises.Count - 1);
    }

    private static string EscapeHtml(string s) =>
        s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
}
