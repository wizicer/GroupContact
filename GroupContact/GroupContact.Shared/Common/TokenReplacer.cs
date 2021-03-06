namespace GroupContact.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    public class TokenReplacer
    {
        private const string Pattern = @"(.*?)(\{.*?\})(.*?)";
        private const int MissingToken = -1;
        private const char StartToken = '{';
        private const char EndToken = '}';
        private const string StartTokenEscaped = "{{";
        private const string EndTokenEscape = "}}";
        private const string TokenFormat = @"{{{0}}}";
        private static readonly StringComparer keyComparer = StringComparer.OrdinalIgnoreCase;
        private IDictionary<string, object> tokenValueDictionary;
        private IFormatProvider formatProvider;
        private string workingInput;

        public string Format(IFormatProvider provider, string input, IDictionary<string, object> tokenValues)
        {
            if (string.IsNullOrEmpty(input)) return input;
            this.FormatPreview(provider, input, tokenValues);
            this.FormatString();
            return workingInput;
        }

        public string FormatPreview(IFormatProvider provider, string input, IDictionary<string, object> tokenValues)
        {
            if (string.IsNullOrEmpty(input)) return input;
            if (tokenValues == null || tokenValues.Count == 0) throw new ArgumentNullException("tokenValues", "The token values dictionary cannot be null or empty.");
            this.formatProvider = provider;
            this.workingInput = input;
            this.tokenValueDictionary = tokenValues;

            this.NormalisedTokenValues();
            this.ReplaceTokensWithValues();
            return workingInput;
        }

        private void NormalisedTokenValues()
        {
            var normalisedTokens = new Dictionary<string, object>(this.tokenValueDictionary.Count, keyComparer);
            foreach (var pair in this.tokenValueDictionary)
            {
                string key = pair.Key;
                if (string.IsNullOrEmpty(key)) continue;

                if (key[0] != StartToken) key = string.Format(TokenFormat, key);

                normalisedTokens.Add(key, pair.Value);
            }
            this.tokenValueDictionary = normalisedTokens;
        }

        private void ReplaceTokensWithValues()
        {
            string[] segments = Regex.Split(this.workingInput, Pattern, RegexOptions.Singleline);
            StringBuilder sb = new StringBuilder();
            foreach (string segment in segments.Where(s => !string.IsNullOrEmpty(s)))
            {
                string segment2 = segment;
                if (segment2.StartsWith(StartTokenEscaped))
                {
                    sb.Append(StartTokenEscaped);
                    segment2 = segment2.Remove(0, StartTokenEscaped.Length);
                }
                if (segment2[0] != StartToken)
                {
                    sb.Append(segment2);
                    continue;
                }

                var tokenPair = this.GetTokenPair(segment2);
                int matchIndex = this.MatchTokenKeyIndex(tokenPair.Key);
                if (matchIndex == MissingToken)
                {
                    sb.Append(string.Format(TokenFormat, segment2));
                    continue;
                }

                this.ExpandFunction(tokenPair);
                this.ExpandFunctionString(tokenPair);
                this.ExpandLazy(tokenPair);
                this.ExpandLazyString(tokenPair);

                sb.Append(string.Format(TokenFormat, matchIndex.ToString() + tokenPair.Value));
            }
            this.workingInput = sb.ToString();
        }

        private KeyValuePair<string, string> GetTokenPair(string token)
        {
            string strippedToken = token.Trim(StartToken, EndToken);
            int index = strippedToken.IndexOfAny(new char[] { ':', ',' });
            if (index == -1) return new KeyValuePair<string, string>(token, null);

            return new KeyValuePair<string, string>(string.Format(TokenFormat, strippedToken.Substring(0, index)), strippedToken.Substring(index));
        }

        private int MatchTokenKeyIndex(string key)
        {
            int counter = 0;
            foreach (var pair in this.tokenValueDictionary)
            {
                if (pair.Key.Equals(key, StringComparison.CurrentCultureIgnoreCase)) return counter;
                counter++;
            }
            return MissingToken;
        }

        private void ExpandFunction(KeyValuePair<string, string> tokenPair)
        {
            Func<string, object> func = this.tokenValueDictionary[tokenPair.Key] as Func<string, object>;
            if (func == null) return;
            this.tokenValueDictionary[tokenPair.Key] = func(tokenPair.Key);
        }

        private void ExpandFunctionString(KeyValuePair<string, string> tokenPair)
        {
            Func<string, string> func = this.tokenValueDictionary[tokenPair.Key] as Func<string, string>;
            if (func == null) return;
            this.tokenValueDictionary[tokenPair.Key] = func(tokenPair.Key);
        }

        private void ExpandLazy(KeyValuePair<string, string> tokenPair)
        {
            Lazy<object> lazy = this.tokenValueDictionary[tokenPair.Key] as Lazy<object>;
            if (lazy == null) return;
            this.tokenValueDictionary[tokenPair.Key] = lazy.Value.ToString();
        }

        private void ExpandLazyString(KeyValuePair<string, string> tokenPair)
        {
            Lazy<string> lazy = this.tokenValueDictionary[tokenPair.Key] as Lazy<string>;
            if (lazy == null) return;
            this.tokenValueDictionary[tokenPair.Key] = lazy.Value;
        }

        private void FormatString()
        {
            try
            {
                this.workingInput = string.Format(this.formatProvider, this.workingInput, this.tokenValueDictionary.Values.ToArray());
            }
            catch (FormatException ex)
            {
                throw new FormatException("Could not format de-tokenised string: " + this.workingInput, ex);
            }
        }
    }
}