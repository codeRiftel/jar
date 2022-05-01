namespace jar {
    public enum Tok { Obj, Arr, Str, Pri }
    public struct Loc {
        public Tok tok;
        public int start;
        public int len;
        public int size;
    }

    public static class Jar {
        public static int Parse(string t, Loc[] locs) {
            int i = 0;
            var (len, size) = PriParse(t, ref i, 0, locs);
            return len;
        }

        private static (int, int) PriParse(string t, ref int i, int len, Loc[] locs) {
            int imlen = 0, l = t.Length;
            while (i < l) switch (t[i]) {
                case ' ':
                case '\t':
                case '\r':
                case '\n':
                    while (i < l && char.IsWhiteSpace(t[i])) i++;
                    break;
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                case '-':
                    var numLoc = new Loc { tok = Tok.Pri, start = i };
                    while (i < l) {
                        var ok = char.IsDigit(t[i]) || t[i] == '-' || t[i] == '+' || t[i] == '.';
                        ok = ok || t[i] == 'E' || t[i] == 'e';
                        if (!ok) break;
                        i++;
                    }
                    numLoc.len = i - numLoc.start;
                    locs[len++] = numLoc;
                    imlen++;
                    break;
                case 't':
                case 'f':
                case 'n':
                    var target = "false";
                    if (t[i] == 't') target = "true"; else if (t[i] == 'n') target = "null";
                    if (string.Compare(t, i, target, 0, target.Length) != 0) return (-1, -1);
                    locs[len++] = new Loc { tok = Tok.Pri, start = i, len = target.Length };
                    i += target.Length;
                    imlen++;
                    break;
                case '"':
                    var strLoc = new Loc { tok = Tok.Str, start = i++ };
                    while (i < l && (t[i] != '"' || (t[i - 1] == '\\' && t[i - 2] != '\\'))) i++;
                    if (i == l) return (-1, -1);
                    strLoc.len = ++i - strLoc.start;
                    locs[len++] = strLoc;
                    imlen++;
                    break;
                case '[':
                case '{':
                    var arrIndex = len++;
                    var tok = Tok.Arr;
                    if (t[i] == '{') tok = Tok.Obj;
                    locs[arrIndex] = new Loc { tok = tok, start = i++ };
                    var size = 0;
                    (len, size) = PriParse(t, ref i, len, locs);
                    locs[arrIndex].len = i - locs[arrIndex].start;
                    locs[arrIndex].size = size;
                    imlen++;
                    break;
                case ']':
                case '}':
                    i++;
                    return (len, imlen);
                default:
                    i++;
                    break;
            }

            return (len, imlen);
        }
    }
}
