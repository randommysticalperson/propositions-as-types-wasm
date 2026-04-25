// =============================================================================
//  TrilogyRules.cs
//  Extended inference rules encoding the full Computational Trilogy.
//
//  Each rule is a static method returning IEnumerable<ProofNode> via yield,
//  demonstrating the three-way correspondence:
//
//    Logic rule  ↔  Type-theoretic term  ↔  Categorical morphism (Func<>)
// =============================================================================

namespace PATBrowser.Core;

public static class TrilogyRules
{
    // Helper: create a proof node with rule, proposition, description, category, and children
    private static DerivedAtom Node(
        string ruleName, string proposition, string description, string category,
        IProofNode[] children)
        => new DerivedAtom($"{proposition}  [{description}]", ruleName, children);

    private static DerivedAtom Node(
        string ruleName, string proposition, string description, string category)
        => new DerivedAtom($"{proposition}  [{description}]", ruleName, Array.Empty<IProofNode>());


    // =========================================================================
    //  SECTION 1: Category Theory Rules (Func<> as morphisms)
    // =========================================================================

    /// Composition (Cut Rule / Substitution)
    /// Logic:       A, A→B, B→C ⊢ C
    /// Type Theory: g ∘ f : A → C
    /// Category:    Hom(A,B) × Hom(B,C) → Hom(A,C)
    public static IEnumerable<ProofNode> Composition<A, B, C>(
        Morphism<A, B> f,
        Morphism<B, C> g)
    {
        var composed = f.Compose(g);
        yield return Node(
            $"Composition (Cut)",
            $"{g.Name} ∘ {f.Name}",
            $"A →^{f.Name} B →^{g.Name} C  ⊢  A →^({g.Name}∘{f.Name}) C",
            "Cut / Substitution",
        new IProofNode[] {
                Node("Morphism f", f.Name, $"f : A → B", "Premise"),
                Node("Morphism g", g.Name, $"g : B → C", "Premise")
            });
    }

    /// Identity Morphism (Reflexivity / id)
    /// Logic:       A ⊢ A
    /// Type Theory: id_A : A → A  (identity function)
    /// Category:    identity morphism  id_A ∈ Hom(A,A)
    public static IEnumerable<ProofNode> IdentityMorphism(string objName)
    {
        yield return Node(
            "Identity (id)",
            $"id_{objName}",
            $"⊢ id_{objName} : {objName} → {objName}",
            "Reflexivity",
        new IProofNode[] {});
    }

    /// Lambda Introduction (→-Intro / counit of hom-tensor adjunction)
    /// Logic:       [A] ⊢ B  ⟹  ⊢ A → B
    /// Type Theory: λx:A. t(x) : A → B
    /// Category:    transpose of f : A×1 → B  under hom-tensor adjunction
    public static IEnumerable<ProofNode> LambdaIntro<A, B>(
        string paramName,
        string bodyType,
        Func<A, B> body)
    {
        var morphism = new Morphism<A, B>($"λ{paramName}.body", body);
        yield return Node(
            "λ-Intro (→-Intro)",
            $"λ{paramName}:A. body : A → {bodyType}",
            $"[{paramName}:A] ⊢ body:{bodyType}  /  ⊢ λ{paramName}.body : A→{bodyType}",
            "→-Introduction / Lambda / Counit of hom-tensor adj",
        new IProofNode[] {Node("Assumption", $"{paramName}:A", $"A", "Hypothesis")});
    }

    /// Application (→-Elim / unit of hom-tensor adjunction / beta)
    /// Logic:       A→B, A ⊢ B
    /// Type Theory: f(a) : B  from f:A→B and a:A
    /// Category:    evaluation morphism  eval : (A→B)×A → B
    public static IEnumerable<ProofNode> Application<A, B>(
        Morphism<A, B> f,
        A argument,
        string argLabel)
    {
        var result = f.Apply(argument);
        yield return Node(
            "Application (→-Elim)",
            $"{f.Name}({argLabel}) = {result}",
            $"f:A→B, a:A  ⊢  f(a):B",
            "→-Elimination / Application / Unit of hom-tensor adj",
        new IProofNode[] {
                Node("Function", f.Name, "A → B", "Premise"),
                Node("Argument", argLabel, "A", "Premise")
            });
    }

    // =========================================================================
    //  SECTION 2: Dependent Type Rules (Σ and Π)
    // =========================================================================

    /// Σ-Introduction (Dependent Sum / Existential)
    /// Logic:       a:A, b:B(a)  ⊢  ∃x:A. B(x)
    /// Type Theory: ⟨a, b⟩ : Σ(x:A).B(x)
    /// Category:    section of the display morphism  B → A
    public static IEnumerable<ProofNode> SigmaIntro<A, B>(
        string aLabel, A a,
        DependentType<A, B> dep)
    {
        var b = dep.At(a);
        var pair = SigmaType<A, B>.Intro(a, dep.Fiber);
        yield return Node(
            "Σ-Intro (∃-Intro)",
            $"⟨{aLabel}, {b}⟩ : Σ(x:{dep.Name}).B(x)",
            $"{aLabel}:A, b:B({aLabel})  ⊢  ⟨{aLabel},b⟩ : Σ(x:A).B(x)",
            "Existential Introduction / Dependent Sum",
        new IProofNode[] {
                Node("Witness", $"{aLabel}:A", "A", "Premise"),
                Node("Fiber", $"b:B({aLabel})", dep.FiberLabel(a), "Premise")
            });
    }

    /// Σ-Elimination (fst / snd projections)
    /// Logic:       ∃x:A.B(x)  ⊢  A  (fst)
    /// Type Theory: fst(⟨a,b⟩) = a
    /// Category:    projection morphisms from the total space
    public static IEnumerable<ProofNode> SigmaElim<A, B>(SigmaType<A, B> pair)
    {
        yield return Node(
            "Σ-Elim fst",
            $"fst({pair}) = {pair.Fst()}",
            $"⟨a,b⟩ : Σ(x:A).B(x)  ⊢  a : A",
            "Existential Elimination (first projection)",
        new IProofNode[] {Node("Pair", pair.ToString(), "Σ(x:A).B(x)", "Premise")});

        yield return Node(
            "Σ-Elim snd",
            $"snd({pair}) = {pair.Snd()}",
            $"⟨a,b⟩ : Σ(x:A).B(x)  ⊢  b : B(a)",
            "Existential Elimination (second projection)",
        new IProofNode[] {Node("Pair", pair.ToString(), "Σ(x:A).B(x)", "Premise")});
    }

    /// Π-Introduction (Dependent Product / Universal)
    /// Logic:       ∀x:A. P(x)  (for all x, prove P(x))
    /// Type Theory: λx:A. f(x) : Π(x:A).B(x)
    /// Category:    section of the dependent product (right adjoint to pullback)
    public static IEnumerable<ProofNode> PiIntro<A, B>(
        string varName,
        DependentType<A, B> dep,
        Func<A, B> proof)
    {
        var pi = PiType<A, B>.Intro($"{varName}:{dep.Name}", proof);
        yield return Node(
            "Π-Intro (∀-Intro)",
            $"λ{varName}. proof : Π({varName}:{dep.Name}).B({varName})",
            $"[{varName}:A] ⊢ proof:B({varName})  /  ⊢ λ{varName}.proof : Π({varName}:A).B",
            "Universal Introduction / Dependent Product / Lambda",
        new IProofNode[] {Node("Body", $"proof : B({varName})", dep.Name, "Hypothesis")});
    }

    /// Π-Elimination (Dependent Product Application / Universal Instantiation)
    /// Logic:       ∀x:A.P(x), a:A  ⊢  P(a)
    /// Type Theory: f(a) : B(a)  from f:Π(x:A).B(x) and a:A
    /// Category:    evaluation at a point
    public static IEnumerable<ProofNode> PiElim<A, B>(
        PiType<A, B> pi,
        A a,
        string aLabel)
    {
        var result = pi.Apply(a);
        yield return Node(
            "Π-Elim (∀-Elim)",
            $"{pi.Name}({aLabel}) = {result}",
            $"f:Π(x:A).B(x), {aLabel}:A  ⊢  f({aLabel}):B({aLabel})",
            "Universal Elimination / Dependent Product Application",
        new IProofNode[] {
                Node("Pi type", pi.Name, "Π(x:A).B(x)", "Premise"),
                Node("Argument", $"{aLabel}:A", "A", "Premise")
            });
    }

    // =========================================================================
    //  SECTION 3: Identity Type Rules
    // =========================================================================

    /// Id-Introduction (Reflexivity)
    /// Logic:       a = a
    /// Type Theory: refl_a : Id_A(a, a)
    /// Category:    diagonal morphism  Δ_A : A → A×A
    public static IEnumerable<ProofNode> IdIntro<A>(A a, string label)
    {
        var refl = IdType<A>.Refl(a);
        yield return Node(
            "Id-Intro (refl)",
            $"refl({label}) : Id_A({label},{label})",
            $"⊢ refl_{label} : Id_A({label},{label})",
            "Reflexivity / Diagonal morphism",
        new IProofNode[] {});
    }

    /// Id-Elimination (J-rule / Path Induction / Transport)
    /// Logic:       a=b, P(a)  ⊢  P(b)
    /// Type Theory: transport along p : Id_A(a,b)
    /// Category:    path lifting / transport in a fibration
    public static IEnumerable<ProofNode> IdElim<A, B>(
        IdType<A> path,
        Func<A, B> motive,
        B baseCase,
        string motiveLabel)
    {
        var result = path.Transport(motive, baseCase);
        yield return Node(
            "Id-Elim (J-rule / Transport)",
            $"transport({path}, {motiveLabel}) = {result}",
            $"p:Id_A(a,b), P(a)  ⊢  P(b)  [transport along p]",
            "Path Induction / J-rule / Transport in fibration",
        new IProofNode[] {
                Node("Path", path.ToString(), "Id_A(a,b)", "Premise"),
                Node("Motive", motiveLabel, "A → Type", "Premise"),
                Node("Base", baseCase!.ToString()!, "P(a)", "Premise")
            });
    }

    // =========================================================================
    //  SECTION 4: Functor and Natural Transformation Rules
    // =========================================================================

    /// Functor-Map (fmap)
    /// Logic:       A→B  ⊢  F(A)→F(B)
    /// Type Theory: fmap f : F(A) → F(B)
    /// Category:    functoriality  F : Hom(A,B) → Hom(F(A),F(B))
    public static IEnumerable<ProofNode> FunctorMap<A>(
        string functorName,
        string morphismName,
        Func<A, A> f,
        Functor<A, A> functor)
    {
        var mapped = functor.MapMorphism(f);
        yield return Node(
            $"Functor-Map ({functorName})",
            $"{functorName}({morphismName}) : {functorName}(A) → {functorName}(B)",
            $"f:A→B  ⊢  {functorName}(f):{functorName}(A)→{functorName}(B)",
            "Functoriality / fmap",
        new IProofNode[] {Node("Morphism", morphismName, "A → B", "Premise")});
    }

    /// NatTransform-Component (naturality)
    /// Logic:       (no direct logical counterpart — structural)
    /// Type Theory: η_A : F(A) → G(A)
    /// Category:    component of a natural transformation
    public static IEnumerable<ProofNode> NatTransformComponent<A>(
        NatTransform<A> eta,
        A fa,
        string faLabel)
    {
        var result = eta.ApplyAt(fa);
        yield return Node(
            $"NatTransform ({eta.Name})",
            $"η_{eta.Name}({faLabel}) = {result}",
            $"η_A : F(A) → G(A)  [naturality square commutes]",
            "Natural transformation component",
        new IProofNode[] {Node("Component input", faLabel, "F(A)", "Premise")});
    }

    // =========================================================================
    //  SECTION 5: Adjunction Rules
    // =========================================================================

    /// Adjunction-Unit (η : A → R(L(A)))
    /// Logic:       A  ⊢  R(L(A))   (introduction rule)
    /// Type Theory: return / pure : A → M(A)
    /// Category:    unit of adjunction L ⊣ R
    public static IEnumerable<ProofNode> AdjunctionUnit<A, B>(
        Adjunction<A, B> adj,
        A a,
        string aLabel)
    {
        var result = adj.ApplyUnit(a);
        yield return Node(
            $"Adj-Unit η ({adj.Name})",
            $"η({aLabel}) = {result}",
            $"a:A  ⊢  η(a):R(L(A))  [unit of L⊣R]",
            "Adjunction unit / return / intro rule",
        new IProofNode[] {Node("Input", $"{aLabel}:A", "A", "Premise")});
    }

    /// Adjunction-Counit (ε : L(R(B)) → B)
    /// Logic:       L(R(B))  ⊢  B   (elimination rule)
    /// Type Theory: extract / join : M(M(A)) → M(A)
    /// Category:    counit of adjunction L ⊣ R
    public static IEnumerable<ProofNode> AdjunctionCounit<A, B>(
        Adjunction<A, B> adj,
        B b,
        string bLabel)
    {
        var result = adj.ApplyCounit(b);
        yield return Node(
            $"Adj-Counit ε ({adj.Name})",
            $"ε({bLabel}) = {result}",
            $"b:L(R(B))  ⊢  ε(b):B  [counit of L⊣R]",
            "Adjunction counit / extract / elim rule",
        new IProofNode[] {Node("Input", $"{bLabel}:L(R(B))", "L(R(B))", "Premise")});
    }

    /// Beta Reduction (zigzag identity)
    /// Logic:       (λx.f(x))(a) ≡ f(a)
    /// Type Theory: computation rule for Π-types
    /// Category:    one of the zigzag identities for hom-tensor adjunction
    public static IEnumerable<ProofNode> BetaReduction<A, B>(
        Adjunction<A, B> adj,
        A a,
        string aLabel)
    {
        var reduces = adj.BetaReduces(a);
        yield return Node(
            "β-Reduction (Zigzag identity)",
            $"ε(η({aLabel})) ≡ {aLabel}  [{(reduces ? "✓ holds" : "✗ fails")}]",
            $"(λx.f(x))(a) ≡ f(a)  [computation rule / zigzag]",
            "Beta reduction / computation rule / zigzag identity",
        new IProofNode[] {Node("Adjunction", adj.Name, "L ⊣ R", "Premise")});
    }

    // =========================================================================
    //  SECTION 6: Monad Rules (Modal Types)
    // =========================================================================

    /// Monad-Return (unit / modal necessity introduction)
    /// Logic:       A  ⊢  □A
    /// Type Theory: return : A → M(A)
    /// Category:    unit of monad
    public static IEnumerable<ProofNode> MonadReturn<A>(A a, string label, string monadName)
    {
        var m = Monad<A>.Return(monadName, a);
        yield return Node(
            $"Monad-Return ({monadName})",
            $"return({label}) = {m}",
            $"{label}:A  ⊢  return({label}):{monadName}(A)",
            "Modal □-Introduction / Monad unit / return",
        new IProofNode[] {Node("Value", $"{label}:A", "A", "Premise")});
    }

    /// Monad-Bind (Kleisli composition / modal elimination)
    /// Logic:       □A, A→□B  ⊢  □B
    /// Type Theory: m >>= f : M(B)
    /// Category:    Kleisli composition
    public static IEnumerable<ProofNode> MonadBind<A, B>(
        Monad<A> ma,
        Func<A, Monad<B>> f,
        string maLabel,
        string fLabel,
        string monadName)
    {
        var result = ma.Bind(f);
        yield return Node(
            $"Monad-Bind ({monadName})",
            $"{maLabel} >>= {fLabel} = {result}",
            $"{maLabel}:{monadName}(A), {fLabel}:A→{monadName}(B)  ⊢  {monadName}(B)",
            "Modal □-Elimination / Monad bind / Kleisli composition",
        new IProofNode[] {
                Node("Monad value", maLabel, $"{monadName}(A)", "Premise"),
                Node("Kleisli arrow", fLabel, $"A → {monadName}(B)", "Premise")
            });
    }
}
