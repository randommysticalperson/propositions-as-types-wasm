// ============================================================
//  DerivedAtom.cs
//  A lightweight proof node for wrapping derived conclusions
//  that need a custom rule label and explicit premises list
//  for proof-tree visualisation.
// ============================================================

namespace PATBrowser.Core;

/// <summary>
/// A named atom that records the rule and premises used to derive it,
/// enabling proof-tree visualisation of derived conclusions.
/// </summary>
public sealed record DerivedAtom(
    string Name,
    string Rule,
    IReadOnlyList<IProofNode> ProofPremises)
    : ProofNode(Rule, Name, ProofPremises);
