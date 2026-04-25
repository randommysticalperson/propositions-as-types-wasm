// ============================================================
//  Propositions.cs
//  Curry-Howard encoding of propositional logic connectives.
//
//  Correspondence table
//  ─────────────────────────────────────────────────────────
//  Logic           Proposition    C# type
//  ─────────────────────────────────────────────────────────
//  Truth           ⊤              TrueProof
//  Falsity         ⊥              FalseProof   (uninhabited)
//  Conjunction     A ∧ B          AndProof<A,B>
//  Disjunction     A ∨ B          OrProof<A,B>  (via OneOf)
//  Implication     A → B          ImpliesProof<A,B>
//  Negation        ¬A             NotProof<A>   (A → ⊥)
//  Biconditional   A ↔ B          IffProof<A,B>
//  ─────────────────────────────────────────────────────────
// ============================================================

using OneOf;

namespace PATBrowser.Core;

// ─── ⊤  Truth ────────────────────────────────────────────────────────────────

/// <summary>
/// The unique proof of truth (⊤).  Corresponds to the unit type.
/// </summary>
public sealed record TrueProof()
    : ProofNode("⊤-Intro", "⊤", Array.Empty<IProofNode>());

// ─── ⊥  Falsity ──────────────────────────────────────────────────────────────

/// <summary>
/// Proof of falsity (⊥).  This type is intentionally uninhabited in normal
/// usage; it can only be produced by <see cref="ExFalso{C}"/>.
/// </summary>
public sealed record FalseProof(string Derivation)
    : ProofNode("⊥", "⊥", Array.Empty<IProofNode>());

// ─── A ∧ B  Conjunction ───────────────────────────────────────────────────────

/// <summary>
/// A proof of <typeparamref name="A"/> ∧ <typeparamref name="B"/>.
/// Corresponds to a product type (pair).
/// </summary>
public sealed record AndProof<A, B>(A Left, B Right, IReadOnlyList<IProofNode> Premises)
    : ProofNode("∧-Intro", $"{Left.Proposition} ∧ {Right.Proposition}", Premises)
    where A : IProofNode
    where B : IProofNode;

// ─── A ∨ B  Disjunction ───────────────────────────────────────────────────────

/// <summary>
/// A proof of <typeparamref name="A"/> ∨ <typeparamref name="B"/>.
/// Backed by <see cref="OneOf{T0,T1}"/> to model the coproduct (sum type).
/// </summary>
public sealed record OrProof<A, B>(
    OneOf<A, B> Value,
    string Tag,
    IReadOnlyList<IProofNode> Premises)
    : ProofNode(Tag, Value.Match(a => $"{a.Proposition} ∨ {typeof(B).Name}", b => $"{typeof(A).Name} ∨ {b.Proposition}"), Premises)
    where A : IProofNode
    where B : IProofNode
{
    /// <summary>Inject a proof of A into A ∨ B (left injection).</summary>
    public static OrProof<A, B> InjectLeft(A proof) =>
        new(OneOf<A, B>.FromT0(proof), "∨-IntroL", new IProofNode[] { proof });

    /// <summary>Inject a proof of B into A ∨ B (right injection).</summary>
    public static OrProof<A, B> InjectRight(B proof) =>
        new(OneOf<A, B>.FromT1(proof), "∨-IntroR", new IProofNode[] { proof });

    /// <summary>
    /// Case analysis (Disjunction Elimination):
    /// given A ∨ B, a proof of A → C, and a proof of B → C, produce C.
    /// </summary>
    public C Match<C>(Func<A, C> caseA, Func<B, C> caseB) =>
        Value.Match(caseA, caseB);
}

// ─── A → B  Implication ───────────────────────────────────────────────────────

/// <summary>
/// A proof of <typeparamref name="A"/> → <typeparamref name="B"/>.
/// Corresponds to a function type (Func&lt;A, B&gt;).
/// </summary>
public sealed record ImpliesProof<A, B>(
    Func<A, B> Arrow,
    string Label,
    IReadOnlyList<IProofNode> Premises)
    : ProofNode("→-Intro", Label, Premises)
    where A : IProofNode
    where B : IProofNode
{
    /// <summary>Apply the implication (Modus Ponens / →-Elim).</summary>
    public B Apply(A premise) => Arrow(premise);
}

// ─── ¬A  Negation ─────────────────────────────────────────────────────────────

/// <summary>
/// A proof of ¬<typeparamref name="A"/>, i.e. A → ⊥.
/// Negation is encoded as a function from A to falsity.
/// </summary>
public sealed record NotProof<A>(
    Func<A, FalseProof> Refutation,
    IReadOnlyList<IProofNode> Premises)
    : ProofNode("¬-Intro", $"¬{typeof(A).Name}", Premises)  // type name used as label; override via DerivedAtom
    where A : IProofNode
{
    /// <summary>Apply the refutation to a proof of A, yielding ⊥.</summary>
    public FalseProof Refute(A proof) => Refutation(proof);
}

// ─── A ↔ B  Biconditional ────────────────────────────────────────────────────

/// <summary>
/// A proof of <typeparamref name="A"/> ↔ <typeparamref name="B"/>.
/// Consists of a forward implication and a backward implication.
/// </summary>
public sealed record IffProof<A, B>(
    ImpliesProof<A, B> Forward,
    ImpliesProof<B, A> Backward,
    IReadOnlyList<IProofNode> Premises)
    : ProofNode("↔-Intro", $"{Forward.Proposition.Split('→')[0].Trim()} ↔ {Backward.Proposition.Split('→')[0].Trim()}", Premises)
    where A : IProofNode
    where B : IProofNode;

// ─── Named atomic propositions ───────────────────────────────────────────────

/// <summary>
/// An atomic proposition identified by name.
/// Represents an assumed (axiomatic) proof of a named proposition.
/// </summary>
public sealed record Atom(string Name)
    : ProofNode("Axiom", Name, Array.Empty<IProofNode>());
