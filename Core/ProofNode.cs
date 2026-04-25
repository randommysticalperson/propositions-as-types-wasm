// ============================================================
//  ProofNode.cs
//  Base infrastructure for proof-tree nodes.
//  Every proof term carries its derivation history so that
//  Spectre.Console can render a full proof tree.
// ============================================================



namespace PATBrowser.Core;

/// <summary>
/// A node in a proof tree.  Every proof term implements this
/// interface so that the visualiser can walk the derivation.
/// </summary>
public interface IProofNode
{
    /// <summary>Human-readable name of the inference rule used.</summary>
    string RuleName { get; }

    /// <summary>The proposition (type) proved by this node.</summary>
    string Proposition { get; }

    /// <summary>Direct premises that were consumed to derive this node.</summary>
    IReadOnlyList<IProofNode> Premises { get; }
}

/// <summary>
/// Convenience base record that stores rule name, proposition label
/// and premises list.
/// </summary>
public abstract record ProofNode(
    string RuleName,
    string Proposition,
    IReadOnlyList<IProofNode> Premises) : IProofNode;
