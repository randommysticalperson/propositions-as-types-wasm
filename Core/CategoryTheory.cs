// =============================================================================
//  CategoryTheory.cs
//  Category-theoretic layer of the Computational Trilogy.
//
//  Encoding (nLab Rosetta Stone):
//    Logic          ↔  Type Theory       ↔  Category Theory
//    proposition    ↔  type              ↔  object
//    proof          ↔  term/program      ↔  generalized element
//    implication    ↔  function type     ↔  morphism  (Func<A,B>)
//    cut rule       ↔  substitution      ↔  composition
//    intro for →    ↔  lambda            ↔  counit of hom-tensor adj
//    elim  for →    ↔  application       ↔  unit of hom-tensor adj
//    beta reduction ↔  computation rule  ↔  zigzag identity
//    eta conversion ↔  uniqueness rule   ↔  other zigzag identity
// =============================================================================

namespace PATBrowser.Core;

// ---------------------------------------------------------------------------
// Obj<A> — an object in a category (the type A itself is the object)
// ---------------------------------------------------------------------------
public sealed record Obj<A>(string Name)
{
    public override string ToString() => Name;
}

// ---------------------------------------------------------------------------
// Morphism<A,B> — a morphism A → B backed by Func<A,B>
//
//   This is the key encoding:  Func<A,B>  ≅  Hom(A,B)
//   • composition  = Func composition  (cut rule / substitution)
//   • identity     = Func.Identity     (reflexivity)
//   • application  = f(a)              (elimination rule for →)
//   • lambda       = a => expr         (introduction rule for →)
// ---------------------------------------------------------------------------
public sealed record Morphism<A, B>(string Name, Func<A, B> Map)
{
    /// Apply the morphism to an element (→-Elimination / function application)
    public B Apply(A a) => Map(a);

    /// Compose  g ∘ this : A → C  (Cut rule / substitution)
    public Morphism<A, C> Compose<C>(Morphism<B, C> g) =>
        new($"({g.Name} ∘ {Name})", a => g.Map(Map(a)));

    /// Identity morphism  id_A : A → A
    public static Morphism<A, A> Identity(string objName) =>
        new($"id_{objName}", a => a);

    public override string ToString() => Name;
}

// ---------------------------------------------------------------------------
// Functor<C,D> — maps objects (types) and morphisms (Func<>) between categories
//
//   F : C → D
//     OnObjects   : C → D          (maps types)
//     OnMorphisms : Hom(A,B) → Hom(F(A),F(B))   (maps Func<A,B> → Func<C,D>)
//
//   Laws (not enforced at runtime, but documented):
//     F(id_A) = id_{F(A)}          (preserves identity)
//     F(g ∘ f) = F(g) ∘ F(f)      (preserves composition)
// ---------------------------------------------------------------------------
public sealed record Functor<C, D>(
    string Name,
    Func<C, D> OnObjects,
    Func<Func<C, C>, Func<D, D>> OnMorphisms)
{
    public D MapObject(C c) => OnObjects(c);
    public Func<D, D> MapMorphism(Func<C, C> f) => OnMorphisms(f);
    public override string ToString() => Name;
}

// ---------------------------------------------------------------------------
// NatTransform<F,G,A> — natural transformation η : F ⟹ G
//
//   Component η_A : F(A) → G(A)
//
//   Naturality square (not enforced, documented):
//     For every f : A → B:
//       η_B ∘ F(f) = G(f) ∘ η_A
// ---------------------------------------------------------------------------
public sealed record NatTransform<A>(
    string Name,
    Func<A, A> Component)
{
    public A ApplyAt(A fa) => Component(fa);
    public override string ToString() => $"η_{Name}";
}

// ---------------------------------------------------------------------------
// Adjunction<A,B> — L ⊣ R adjunction
//
//   Unit    η : A → R(L(A))
//   Counit  ε : L(R(B)) → B
//
//   Corresponds to:
//     Logic:       introduction / elimination rule for implication
//     Type Theory: lambda / application
//     Category:    counit / unit of hom-tensor adjunction
// ---------------------------------------------------------------------------
public sealed record Adjunction<A, B>(
    string Name,
    Func<A, B> Unit,       // η : A → R(L(A))  — intro rule / lambda
    Func<B, A> Counit)     // ε : L(R(B)) → B  — elim rule / application
{
    /// Unit  (→-Introduction / lambda abstraction)
    public B ApplyUnit(A a) => Unit(a);

    /// Counit  (→-Elimination / function application)
    public A ApplyCounit(B b) => Counit(b);

    /// Beta reduction: (λx.f(x))(a) ≡ f(a)  — zigzag identity
    public bool BetaReduces(A a) =>
        EqualityComparer<A>.Default.Equals(ApplyCounit(ApplyUnit(a)), a);

    public override string ToString() => $"{Name}: L ⊣ R";
}

// ---------------------------------------------------------------------------
// DependentType<A,B> — a type B(a) dependent on a : A
//
//   Corresponds to:
//     Logic:       predicate P(x)
//     Type Theory: dependent type  x:A ⊢ B(x) : Type
//     Category:    display morphism  B → A  (family of objects over A)
// ---------------------------------------------------------------------------
public sealed record DependentType<A, B>(
    string Name,
    Func<A, string> FiberName,
    Func<A, B> Fiber)
{
    public B At(A a) => Fiber(a);
    public string FiberLabel(A a) => FiberName(a);
    public override string ToString() => $"x:{Name} ⊢ B(x)";
}

// ---------------------------------------------------------------------------
// SigmaType<A,B> — Dependent Sum  Σ(x:A).B(x)
//
//   Corresponds to:
//     Logic:       existential quantification  ∃x:A. P(x)
//     Type Theory: dependent sum type
//     Category:    dependent sum / total space  (left adjoint to pullback)
//
//   Introduction: ⟨a, b⟩ where b : B(a)
//   Elimination:  fst, snd projections
// ---------------------------------------------------------------------------
public sealed record SigmaType<A, B>(A First, B Second)
{
    /// Σ-Introduction: pair ⟨a, b(a)⟩
    public static SigmaType<A, B> Intro(A a, Func<A, B> b) =>
        new(a, b(a));

    /// Σ-Elimination (fst): project the first component
    public A Fst() => First;

    /// Σ-Elimination (snd): project the second component
    public B Snd() => Second;

    public override string ToString() => $"⟨{First}, {Second}⟩";
}

// ---------------------------------------------------------------------------
// PiType<A,B> — Dependent Product  Π(x:A).B(x)
//
//   Corresponds to:
//     Logic:       universal quantification  ∀x:A. P(x)
//     Type Theory: dependent product type  (generalised function type)
//     Category:    dependent product  (right adjoint to pullback)
//
//   Introduction: λx. f(x)  where f(x) : B(x) for all x
//   Elimination:  apply to a specific a : A  →  f(a) : B(a)
// ---------------------------------------------------------------------------
public sealed record PiType<A, B>(string Name, Func<A, B> Lambda)
{
    /// Π-Introduction: λ-abstraction  (introduction rule for →)
    public static PiType<A, B> Intro(string name, Func<A, B> f) =>
        new(name, f);

    /// Π-Elimination: function application  (elimination rule for →)
    public B Apply(A a) => Lambda(a);

    public override string ToString() => $"Π({Name})";
}

// ---------------------------------------------------------------------------
// IdType<A> — Identity / Path type  Id_A(a, b)
//
//   Corresponds to:
//     Logic:       propositional equality
//     Type Theory: identity type / path type
//     Category:    path space object  (diagonal morphism Δ : A → A×A)
//
//   Introduction: refl_a : Id_A(a, a)   (reflexivity)
//   Elimination:  J-rule / path induction / transport
// ---------------------------------------------------------------------------
public sealed record IdType<A>(A Left, A Right, bool IsRefl)
{
    /// Id-Introduction: reflexivity  refl : Id_A(a,a)
    public static IdType<A> Refl(A a) => new(a, a, true);

    /// Id-Elimination: J-rule — transport P(a) along p : Id_A(a,b) to get P(b)
    public B Transport<B>(Func<A, B> motive, B baseCase) =>
        IsRefl ? baseCase : motive(Right);

    public override string ToString() =>
        IsRefl ? $"refl({Left})" : $"Id({Left}, {Right})";
}

// ---------------------------------------------------------------------------
// Monad<A> — a monad (modal type / closure operator)
//
//   Corresponds to:
//     Logic:       modality  □A  (necessity)
//     Type Theory: modal type theory
//     Category:    (idempotent) monad on a category
//
//   Unit  (return):  A → M(A)
//   Bind  (>>=):     M(A) → (A → M(B)) → M(B)
// ---------------------------------------------------------------------------
public sealed record Monad<A>(string Name, A Value)
{
    /// Monad unit (return / pure): A → M(A)
    public static Monad<A> Return(string name, A a) => new(name, a);

    /// Monad bind (>>=): M(A) → (A → M(B)) → M(B)
    public Monad<B> Bind<B>(Func<A, Monad<B>> f) => f(Value);

    /// Monad map (fmap): M(A) → (A → B) → M(B)
    public Monad<B> Map<B>(Func<A, B> f) => new(Name, f(Value));

    public override string ToString() => $"{Name}({Value})";
}
