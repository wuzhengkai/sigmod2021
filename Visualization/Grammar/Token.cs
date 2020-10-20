using System;

namespace Microsoft.ProgramSynthesis.Visualization.Grammar {
    /// <summary>
    ///     The class to represent tokens.
    /// </summary>
    public class Token : IEquatable<Token> {
        /// <summary>
        ///     Creates a token from name.
        /// </summary>
        /// <param name="name"> Name </param>
        public Token(string name) { Name = name; }
        /// <summary>
        ///     Token name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Equality of token.
        /// </summary>
        /// <param name="other"> The other token compared. </param>
        /// <returns></returns>
        public bool Equals(Token other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return string.Equals(Name, other.Name);
        }

        /// <inheritdoc />
        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((Token) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode() => Name != null ? Name.GetHashCode() : 0;

        /// <summary>
        ///     Checks if two tokens equal.
        /// </summary>
        /// <param name="left"> First token </param>
        /// <param name="right"> Second token </param>
        /// <returns> True if equal, otherwise false </returns>
        public static bool operator ==(Token left, Token right) => Equals(left, right);

        /// <summary>
        ///     Checks if two tokens are unequal.
        /// </summary>
        /// <param name="left"> First token </param>
        /// <param name="right"> Second token </param>
        /// <returns> True if unequal, otherwise false </returns>
        public static bool operator !=(Token left, Token right) => !Equals(left, right);

        /// <inheritdoc />
        public override string ToString() => Name;
    }

    /// <summary>
    ///     Constant is a special token with fixed value.
    /// </summary>
    public class Constant : Token {
        /// <summary>
        ///     Creates constant from name.
        /// </summary>
        /// <param name="name"> Name </param>
        public Constant(string name) : base(name) { }
    }
}