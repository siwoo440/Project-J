using System;
using System.Text;

namespace ProjectJ.Networking.Fusion
{
    public static class ProjectJFusionRoomCode
    {
        public const int Length = 6;

        private const string SessionPrefix =
            "ProjectJ-";

        private const string Alphabet =
            "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

        public static string Generate()
        {
            byte[] bytes =
                Guid.NewGuid()
                    .ToByteArray();

            StringBuilder builder =
                new StringBuilder(
                    Length
                );

            for (
                int i = 0;
                i < Length;
                i++
            )
            {
                int index =
                    bytes[i] %
                    Alphabet.Length;

                builder.Append(
                    Alphabet[index]
                );
            }

            return builder.ToString();
        }

        public static bool TryNormalize(
            string value,
            out string normalized,
            out string errorMessage
        )
        {
            normalized =
                string.IsNullOrWhiteSpace(value)
                    ? string.Empty
                    : value
                        .Trim()
                        .ToUpperInvariant();

            if (
                normalized.Length !=
                Length
            )
            {
                errorMessage =
                    $"방 코드는 {Length}자리여야 합니다.";

                return false;
            }

            for (
                int i = 0;
                i < normalized.Length;
                i++
            )
            {
                if (
                    Alphabet.IndexOf(
                        normalized[i]
                    ) >= 0
                )
                {
                    continue;
                }

                errorMessage =
                    "방 코드에 사용할 수 없는 문자가 포함되어 있습니다.";

                return false;
            }

            errorMessage =
                string.Empty;

            return true;
        }

        public static string ToSessionName(
            string normalizedCode
        )
        {
            return
                SessionPrefix +
                normalizedCode;
        }

        public static bool TryExtractFromSessionName(
            string sessionName,
            out string roomCode
        )
        {
            roomCode =
                string.Empty;

            if (
                string.IsNullOrEmpty(
                    sessionName
                ) ||
                !sessionName.StartsWith(
                    SessionPrefix,
                    StringComparison.Ordinal
                )
            )
            {
                return false;
            }

            string candidate =
                sessionName.Substring(
                    SessionPrefix.Length
                );

            return TryNormalize(
                candidate,
                out roomCode,
                out _
            );
        }
    }
}
