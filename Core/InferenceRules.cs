// ============================================================
//  InferenceRules.cs
//  All standard propositional inference rules encoded as
//  static methods that use `yield return` to lazily emit
//  derivations.  Each method corresponds to a named rule
//  from the Wikipedia "List of rules of inference".
//
//  Rules implemented
//  ─────────────────────────────────────────────────────────
//  Propositional
//    Conjunction    : ∧-Intro, ∧-ElimL, ∧-ElimR
//    Disjunction    : ∨-IntroL, ∨-IntroR, ∨-Elim (Case Analysis)
//                     Disjunctive Syllogism
//                     Constructive Dilemma
//    Implication    : →-Intro (Deduction), →-Elim (Modus Ponens)
//                     Modus Tollens
//                     Hypothetical Syllogism
//                     Absorption
//    Negation       : ¬-Intro (Reductio), Ex Contradictione (⊥-Elim)
//                     Double Negation Elimination
//    Biconditional  : ↔-Intro, ↔-ElimL, ↔-ElimR
//  ─────────────────────────────────────────────────────────
// ============================================================

namespace PATBrowser.Core;

/// <summary>
/// Static class containing all inference rules as iterator methods.
/// Each rule returns <see cref="IEnumerable{T}"/> so that callers can
/// lazily enumerate all valid derivations (useful for proof search).
/// </summary>
public static class InferenceRules
{
    // ═══════════════════════════════════════════════════════
    //  CONJUNCTION  (∧)
    // ═══════════════════════════════════════════════════════

    /// <summary>
    /// Conjunction Introduction (Adjunction):
    ///   A    B
    ///   ─────── ∧-Intro
    ///   A ∧ B
    /// </summary>
    public static IEnumerable<AndProof<A, B>> ConjunctionIntro<A, B>(A a, B b)
        where A : IProofNode
        where B : IProofNode
    {
        yield return new AndProof<A, B>(a, b, new IProofNode[] { a, b });
    }

    /// <summary>
    /// Conjunction Elimination – Left (Simplification):
    ///   A ∧ B
    ///   ───── ∧-ElimL
    ///     A
    /// </summary>
    public static IEnumerable<A> ConjunctionElimLeft<A, B>(AndProof<A, B> and)
        where A : IProofNode
        where B : IProofNode
    {
        yield return and.Left;
    }

    /// <summary>
    /// Conjunction Elimination – Right (Simplification):
    ///   A ∧ B
    ///   ───── ∧-ElimR
    ///     B
    /// </summary>
    public static IEnumerable<B> ConjunctionElimRight<A, B>(AndProof<A, B> and)
        where A : IProofNode
        where B : IProofNode
    {
        yield return and.Right;
    }

    // ═══════════════════════════════════════════════════════
    //  DISJUNCTION  (∨)
    // ═══════════════════════════════════════════════════════

    /// <summary>
    /// Disjunction Introduction – Left (Addition):
    ///     A
    ///   ───── ∨-IntroL
    ///   A ∨ B
    /// </summary>
    public static IEnumerable<OrProof<A, B>> DisjunctionIntroLeft<A, B>(A a)
        where A : IProofNode
        where B : IProofNode
    {
        yield return OrProof<A, B>.InjectLeft(a);
    }

    /// <summary>
    /// Disjunction Introduction – Right (Addition):
    ///     B
    ///   ───── ∨-IntroR
    ///   A ∨ B
    /// </summary>
    public static IEnumerable<OrProof<A, B>> DisjunctionIntroRight<A, B>(B b)
        where A : IProofNode
        where B : IProofNode
    {
        yield return OrProof<A, B>.InjectRight(b);
    }

    /// <summary>
    /// Disjunction Elimination (Case Analysis / Proof by Cases):
    ///   A → C    B → C    A ∨ B
    ///   ─────────────────────── ∨-Elim
    ///               C
    /// </summary>
    public static IEnumerable<C> DisjunctionElim<A, B, C>(
        OrProof<A, B> or,
        ImpliesProof<A, C> caseA,
        ImpliesProof<B, C> caseB)
        where A : IProofNode
        where B : IProofNode
        where C : IProofNode
    {
        yield return or.Match(caseA.Apply, caseB.Apply);
    }

    /// <summary>
    /// Disjunctive Syllogism:
    ///   A ∨ B    ¬A
    ///   ─────────── DS
    ///       B
    /// </summary>
    public static IEnumerable<B> DisjunctiveSyllogismLeft<A, B>(
        OrProof<A, B> or,
        NotProof<A> notA)
        where A : IProofNode
        where B : IProofNode
    {
        // We can only yield if the disjunction is actually inhabited on the right.
        // In a constructive setting we must inspect the value.
        if (or.Value.IsT1)
            yield return or.Value.AsT1;
    }

    /// <summary>
    /// Disjunctive Syllogism (symmetric):
    ///   A ∨ B    ¬B
    ///   ─────────── DS
    ///       A
    /// </summary>
    public static IEnumerable<A> DisjunctiveSyllogismRight<A, B>(
        OrProof<A, B> or,
        NotProof<B> notB)
        where A : IProofNode
        where B : IProofNode
    {
        if (or.Value.IsT0)
            yield return or.Value.AsT0;
    }

    /// <summary>
    /// Constructive Dilemma:
    ///   A → C    B → D    A ∨ B
    ///   ─────────────────────── CD
    ///           C ∨ D
    /// </summary>
    public static IEnumerable<OrProof<C, D>> ConstructiveDilemma<A, B, C, D>(
        OrProof<A, B> or,
        ImpliesProof<A, C> ac,
        ImpliesProof<B, D> bd)
        where A : IProofNode
        where B : IProofNode
        where C : IProofNode
        where D : IProofNode
    {
        yield return or.Match(
            a => OrProof<C, D>.InjectLeft(ac.Apply(a)),
            b => OrProof<C, D>.InjectRight(bd.Apply(b)));

    }

    // ═══════════════════════════════════════════════════════
    //  IMPLICATION  (→)
    // ═══════════════════════════════════════════════════════

    /// <summary>
    /// Implication Introduction (Deduction Theorem / Conditional Introduction):
    ///   [A] ⊢ B
    ///   ─────── →-Intro
    ///   A → B
    /// Wraps a function from A to B as a proof of A → B.
    /// </summary>
    public static IEnumerable<ImpliesProof<A, B>> ImplicationIntro<A, B>(
        Func<A, B> deduction,
        string label = "→-Intro")
        where A : IProofNode
        where B : IProofNode
    {
        yield return new ImpliesProof<A, B>(deduction, label, Array.Empty<IProofNode>());
    }

    /// <summary>
    /// Modus Ponens (Implication Elimination / →-Elim):
    ///   A → B    A
    ///   ───────── MP
    ///       B
    /// </summary>
    public static IEnumerable<B> ModusPonens<A, B>(
        ImpliesProof<A, B> implication,
        A premise)
        where A : IProofNode
        where B : IProofNode
    {
        yield return implication.Apply(premise);
    }

    /// <summary>
    /// Modus Tollens:
    ///   A → B    ¬B
    ///   ─────────── MT
    ///       ¬A
    /// </summary>
    public static IEnumerable<NotProof<A>> ModusTollens<A, B>(
        ImpliesProof<A, B> implication,
        NotProof<B> notB)
        where A : IProofNode
        where B : IProofNode
    {
        yield return new NotProof<A>(
            a => notB.Refute(implication.Apply(a)),
            new IProofNode[] { implication, notB });
    }

    /// <summary>
    /// Hypothetical Syllogism (Transitivity of Implication):
    ///   A → B    B → C
    ///   ─────────────── HS
    ///       A → C
    /// </summary>
    public static IEnumerable<ImpliesProof<A, C>> HypotheticalSyllogism<A, B, C>(
        ImpliesProof<A, B> ab,
        ImpliesProof<B, C> bc)
        where A : IProofNode
        where B : IProofNode
        where C : IProofNode
    {
        yield return new ImpliesProof<A, C>(
            a => bc.Apply(ab.Apply(a)),
            "HS",
            new IProofNode[] { ab, bc });
    }

    /// <summary>
    /// Absorption:
    ///   A → B
    ///   ───────────── Abs
    ///   A → (A ∧ B)
    /// </summary>
    public static IEnumerable<ImpliesProof<A, AndProof<A, B>>> Absorption<A, B>(
        ImpliesProof<A, B> ab)
        where A : IProofNode
        where B : IProofNode
    {
        yield return new ImpliesProof<A, AndProof<A, B>>(
            a => new AndProof<A, B>(a, ab.Apply(a), new IProofNode[] { a }),
            $"{ab.Proposition.Split("→")[0].Trim()} → ({ab.Proposition.Split("→")[0].Trim()} ∧ {ab.Proposition.Split("→").Last().Trim()})",
            new IProofNode[] { ab });
    }

    // ═══════════════════════════════════════════════════════
    //  NEGATION  (¬)
    // ═══════════════════════════════════════════════════════

    /// <summary>
    /// Negation Introduction (Reductio ad Absurdum):
    ///   [A] ⊢ B    [A] ⊢ ¬B
    ///   ─────────────────── ¬-Intro
    ///           ¬A
    /// Wraps a refutation function as a proof of ¬A.
    /// </summary>
    public static IEnumerable<NotProof<A>> NegationIntro<A>(
        Func<A, FalseProof> refutation)
        where A : IProofNode
    {
        yield return new NotProof<A>(refutation, Array.Empty<IProofNode>());
    }

    /// <summary>
    /// Ex Contradictione Quodlibet (Explosion / ⊥-Elim):
    ///   A    ¬A
    ///   ─────── ECQ
    ///     C
    /// From a contradiction, anything follows.
    /// </summary>
    public static IEnumerable<C> ExFalso<A, C>(A proof, NotProof<A> notProof, Func<FalseProof, C> explosion)
        where A : IProofNode
        where C : IProofNode
    {
        var contradiction = notProof.Refute(proof);
        yield return explosion(contradiction);
    }

    /// <summary>
    /// Double Negation Elimination (classical):
    ///   ¬¬A
    ///   ─── DNE
    ///    A
    /// Note: this rule is classical (not constructively valid in general).
    /// We model it by requiring an explicit witness function.
    /// </summary>
    public static IEnumerable<A> DoubleNegationElim<A>(
        NotProof<NotProof<A>> notNotA,
        Func<NotProof<NotProof<A>>, A> classicalWitness)
        where A : IProofNode
    {
        yield return classicalWitness(notNotA);
    }

    // ═══════════════════════════════════════════════════════
    //  BICONDITIONAL  (↔)
    // ═══════════════════════════════════════════════════════

    /// <summary>
    /// Biconditional Introduction:
    ///   A → B    B → A
    ///   ─────────────── ↔-Intro
    ///       A ↔ B
    /// </summary>
    public static IEnumerable<IffProof<A, B>> BiconditionalIntro<A, B>(
        ImpliesProof<A, B> forward,
        ImpliesProof<B, A> backward)
        where A : IProofNode
        where B : IProofNode
    {
        yield return new IffProof<A, B>(forward, backward, new IProofNode[] { forward, backward });
    }

    /// <summary>
    /// Biconditional Elimination – Forward:
    ///   A ↔ B    A
    ///   ───────── ↔-ElimL
    ///       B
    /// </summary>
    public static IEnumerable<B> BiconditionalElimForward<A, B>(IffProof<A, B> iff, A a)
        where A : IProofNode
        where B : IProofNode
    {
        yield return iff.Forward.Apply(a);
    }

    /// <summary>
    /// Biconditional Elimination – Backward:
    ///   A ↔ B    B
    ///   ───────── ↔-ElimR
    ///       A
    /// </summary>
    public static IEnumerable<A> BiconditionalElimBackward<A, B>(IffProof<A, B> iff, B b)
        where A : IProofNode
        where B : IProofNode
    {
        yield return iff.Backward.Apply(b);
    }
}
