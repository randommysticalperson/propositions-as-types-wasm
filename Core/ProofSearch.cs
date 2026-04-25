// ============================================================
//  ProofSearch.cs
//  A lazy, yield-based proof search engine.
//
//  ProofSearch demonstrates how `yield return` enables
//  non-deterministic, lazy enumeration of all derivations
//  that can be constructed from a given set of assumptions
//  using the inference rules.
//
//  The engine works over a "proof context" — a list of
//  named atomic propositions — and tries to derive a target
//  proposition by systematically applying rules.
// ============================================================

namespace PATBrowser.Core;

/// <summary>
/// A named assumption in the proof context.
/// </summary>
public record Assumption(string Name, Atom Proof);

/// <summary>
/// A proof context (Γ) — a finite set of named assumptions.
/// </summary>
public class ProofContext
{
    private readonly List<Assumption> _assumptions = new();

    public IReadOnlyList<Assumption> Assumptions => _assumptions;

    public ProofContext Assume(string name)
    {
        _assumptions.Add(new Assumption(name, new Atom(name)));
        return this;
    }

    public IEnumerable<Atom> GetAtoms() => _assumptions.Select(a => a.Proof);
}

/// <summary>
/// Lazy proof search over atomic propositions using yield.
/// Demonstrates non-deterministic derivation enumeration.
/// </summary>
public static class ProofSearch
{
    // ─── Enumerate all conjunctions derivable from context ───────────────────

    /// <summary>
    /// Lazily yield all pairwise conjunctions derivable from the atoms
    /// in the given context.  Uses <c>yield return</c> to produce each
    /// derivation on demand without materialising the full set.
    /// </summary>
    public static IEnumerable<AndProof<Atom, Atom>> AllConjunctions(ProofContext ctx)
    {
        var atoms = ctx.GetAtoms().ToList();
        foreach (var a in atoms)
            foreach (var b in atoms)
                foreach (var conj in InferenceRules.ConjunctionIntro(a, b))
                    yield return conj;
    }

    // ─── Enumerate all disjunctions derivable from context ───────────────────

    /// <summary>
    /// Lazily yield all disjunctions A ∨ B where A and B are atoms in context.
    /// Both left and right injections are emitted.
    /// </summary>
    public static IEnumerable<OrProof<Atom, Atom>> AllDisjunctions(ProofContext ctx)
    {
        var atoms = ctx.GetAtoms().ToList();
        foreach (var a in atoms)
            foreach (var b in atoms)
            {
                foreach (var d in InferenceRules.DisjunctionIntroLeft<Atom, Atom>(a))
                    yield return d;
                foreach (var d in InferenceRules.DisjunctionIntroRight<Atom, Atom>(b))
                    yield return d;
            }
    }

    // ─── Enumerate all hypothetical syllogisms from a chain ──────────────────

    /// <summary>
    /// Given a list of implication proofs, lazily yield all transitive
    /// compositions derivable by Hypothetical Syllogism.
    /// This demonstrates how <c>yield</c> enables lazy chain building.
    /// </summary>
    public static IEnumerable<ImpliesProof<Atom, Atom>> AllHypotheticalSyllogisms(
        IReadOnlyList<ImpliesProof<Atom, Atom>> implications)
    {
        // First yield all direct implications
        foreach (var impl in implications)
            yield return impl;

        // Then yield all two-step compositions
        foreach (var ab in implications)
            foreach (var bc in implications)
                if (ab.Proposition.EndsWith(bc.Proposition.Split('→')[0].Trim()))
                    foreach (var ac in InferenceRules.HypotheticalSyllogism(ab, bc))
                        yield return ac;
    }

    // ─── Enumerate all Modus Ponens applications ─────────────────────────────

    /// <summary>
    /// Given a set of implications and a set of atoms (premises),
    /// lazily yield all conclusions derivable by Modus Ponens.
    /// </summary>
    public static IEnumerable<(Atom Conclusion, string Derivation)> AllModusPonens(
        IReadOnlyList<ImpliesProof<Atom, Atom>> implications,
        IReadOnlyList<Atom> atoms)
    {
        foreach (var impl in implications)
            foreach (var atom in atoms)
                if (impl.Proposition.StartsWith(atom.Name + " →"))
                    foreach (var conclusion in InferenceRules.ModusPonens(impl, atom))
                        yield return (conclusion, $"MP({atom.Name}, {impl.Proposition})");
    }
}
