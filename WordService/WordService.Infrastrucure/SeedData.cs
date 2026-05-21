using WordService.Domain.Entity;

namespace WordService.Infrastructure;

public static class SeedData
{
    public static WordRoot[] GetWordRoots()
    {
        var roots = new List<WordRoot>();

        // 1: a-/an-
        var wr1 = new WordRoot(1, "a-/an-", "Greek", "不，无，非", "a", "a-/an- 来自希腊语，表示「没有、缺乏」。想象一个「anonymous（匿名的）」人，就是没有（an-）名字（onym）的人。这个前缀在医学术语中特别常见，比如 anemia（贫血）= 没有（an-）血（emia）。");
        wr1.Examples.Add(new WordRootExample(wr1.Id, "amoral", "a", "moral", "", "非道德的", "不道德的→非道德的"));
        wr1.Examples.Add(new WordRootExample(wr1.Id, "apolitical", "a", "political", "", "不关政治的", "不关心政治→不关政治的"));
        wr1.Examples.Add(new WordRootExample(wr1.Id, "anonymous", "an", "onym", "ous", "匿名的", "没有名字→匿名的"));
        wr1.Examples.Add(new WordRootExample(wr1.Id, "anarchy", "an", "archy", "", "无政府状态", "没有统治→无政府"));
        wr1.Quizzes.Add(new WordRootQuiz(wr1.Id, "amoral 的意思是什么？", new[] { "非道德的", "无政府状态", "不关政治的", "匿名的" }, 0));
        roots.Add(wr1);

        // 2: anti-
        var wr2 = new WordRoot(2, "anti-", "Greek", "反对，相反", "anti", "anti- 源自希腊语 anti「对抗」。记住「抗生素 antibiotic」这个词：anti-（对抗）+ bio（生命）+ -tic（的），就是「对抗生命的」→ 专门对抗细菌的。从此你就能理解为什么「抗体 antibody」、「反战 antiwar」都用这个前缀。");
        wr2.Examples.Add(new WordRootExample(wr2.Id, "antiwar", "anti", "war", "", "反战的", "反对战争→反战的"));
        wr2.Examples.Add(new WordRootExample(wr2.Id, "antibody", "anti", "body", "", "抗体", "对抗物→抗体"));
        wr2.Examples.Add(new WordRootExample(wr2.Id, "antipathy", "anti", "pathy", "", "反感", "相反的感情→反感"));
        wr2.Examples.Add(new WordRootExample(wr2.Id, "antithesis", "anti", "thesis", "", "对立", "相反的观点→对立"));
        wr2.Quizzes.Add(new WordRootQuiz(wr2.Id, "antiwar 的意思是什么？", new[] { "反战的", "反感", "抗体", "对立" }, 0));
        roots.Add(wr2);

        // 3: counter-
        var wr3 = new WordRoot(3, "counter-", "Latin", "相反，对抗", "counter", "counter- 来自拉丁语，字面意思「相反方向」。想象拳击场上的「反击 counterattack」：counter-（反向）+ attack（攻击）。商店柜台叫 counter，因为是面对面（counter）站着交易的地方。");
        wr3.Examples.Add(new WordRootExample(wr3.Id, "counteract", "counter", "act", "", "对抗", "相反行动→对抗"));
        wr3.Examples.Add(new WordRootExample(wr3.Id, "counterbalance", "counter", "balance", "", "平衡", "反向平衡→平衡"));
        wr3.Examples.Add(new WordRootExample(wr3.Id, "counterfeit", "counter", "feit", "", "伪造", "相反的制作→伪造"));
        wr3.Examples.Add(new WordRootExample(wr3.Id, "counterpart", "counter", "part", "", "对应物", "对立的部分→对应物"));
        wr3.Quizzes.Add(new WordRootQuiz(wr3.Id, "counteract 的意思是什么？", new[] { "平衡", "对应物", "伪造", "对抗" }, 3));
        roots.Add(wr3);

        // 4: de-
        var wr4 = new WordRoot(4, "de-", "Latin", "向下，去除", "de", "de- 有两个核心含义：①向下（descend下降）②去除（debug除错）。记住 decline 这个词：de-（向下）+ cline（倾斜）= 向下倾斜 → 衰退、拒绝。掌握这个前缀，200+单词轻松记。");
        wr4.Examples.Add(new WordRootExample(wr4.Id, "descend", "de", "scend", "", "下降", "向下走→下降"));
        wr4.Examples.Add(new WordRootExample(wr4.Id, "devalue", "de", "value", "", "贬值", "去掉价值→贬值"));
        wr4.Examples.Add(new WordRootExample(wr4.Id, "deforest", "de", "forest", "", "砍伐森林", "去除森林→砍伐"));
        wr4.Examples.Add(new WordRootExample(wr4.Id, "decode", "de", "code", "", "解码", "去掉密码→解码"));
        wr4.Quizzes.Add(new WordRootQuiz(wr4.Id, "descend 的意思是什么？", new[] { "解码", "贬值", "砍伐森林", "下降" }, 3));
        roots.Add(wr4);

        // 5: dis-
        var wr5 = new WordRoot(5, "dis-", "Latin", "不，分开", "dis", "dis- 表示「分离、相反」。记住 discover（发现）这个词：dis-（去掉）+ cover（覆盖物）= 把盖子揭开 → 发现。disappear = dis-（相反）+ appear（出现）→ 消失。这个前缀是英语中使用频率最高的否定前缀之一。");
        wr5.Examples.Add(new WordRootExample(wr5.Id, "disagree", "dis", "agree", "", "不同意", "不一致→不同意"));
        wr5.Examples.Add(new WordRootExample(wr5.Id, "disappear", "dis", "appear", "", "消失", "不出现→消失"));
        wr5.Examples.Add(new WordRootExample(wr5.Id, "disconnect", "dis", "connect", "", "断开", "不连接→断开"));
        wr5.Examples.Add(new WordRootExample(wr5.Id, "disorder", "dis", "order", "", "混乱", "无秩序→混乱"));
        wr5.Quizzes.Add(new WordRootQuiz(wr5.Id, "disagree 的意思是什么？", new[] { "断开", "混乱", "不同意", "消失" }, 2));
        roots.Add(wr5);

        // 6: ex-/e-
        var wr6 = new WordRoot(6, "ex-/e-", "Latin", "出，外", "ex", "ex-/e- 表示「向外」。记住 exit（出口）：ex-（向外）+ it（走）= 向外走的地方。export（出口）= ex-（向外）+ port（运）→ 运出去。元音前会省略x变成e-，如 emit（发射）。");
        wr6.Examples.Add(new WordRootExample(wr6.Id, "export", "ex", "port", "", "出口", "带出去→出口"));
        wr6.Examples.Add(new WordRootExample(wr6.Id, "exit", "ex", "it", "", "出口", "走出去→出口"));
        wr6.Examples.Add(new WordRootExample(wr6.Id, "emerge", "e", "merge", "", "浮现", "出现→浮现"));
        wr6.Examples.Add(new WordRootExample(wr6.Id, "evade", "e", "vade", "", "逃避", "走出去→逃避"));
        wr6.Quizzes.Add(new WordRootQuiz(wr6.Id, "export 的意思是什么？", new[] { "浮现", "出口", "出口", "逃避" }, 1));
        roots.Add(wr6);

        // 7: in-/im-
        var wr7 = new WordRoot(7, "in-/im-", "Latin", "进入，使", "in", "in-/im- 有两个相反的意思，要根据词根判断：①进入（insert插入）②否定（impossible不可能）。辅音m/p/b前会变成im-，这是为了发音方便。记住：如果词根是形容词，in-通常表否定；如果是动词，通常表「进入」。");
        wr7.Examples.Add(new WordRootExample(wr7.Id, "import", "im", "port", "", "进口", "带进来→进口"));
        wr7.Examples.Add(new WordRootExample(wr7.Id, "income", "in", "come", "", "收入", "进来的→收入"));
        wr7.Examples.Add(new WordRootExample(wr7.Id, "insert", "in", "sert", "", "插入", "放进去→插入"));
        wr7.Examples.Add(new WordRootExample(wr7.Id, "invade", "in", "vade", "", "入侵", "走进去→入侵"));
        wr7.Quizzes.Add(new WordRootQuiz(wr7.Id, "import 的意思是什么？", new[] { "收入", "进口", "插入", "入侵" }, 1));
        roots.Add(wr7);

        // 8: inter-
        var wr8 = new WordRoot(8, "inter-", "Latin", "在...之间", "inter", "inter- 来自拉丁语「在...之间」。internet（互联网）= inter-（相互之间）+ net（网）→ 相互连接的网络。international（国际的）= inter-（之间）+ national（国家的）→ 国与国之间的。");
        wr8.Examples.Add(new WordRootExample(wr8.Id, "international", "inter", "national", "", "国际的", "国家之间→国际的"));
        wr8.Examples.Add(new WordRootExample(wr8.Id, "internet", "inter", "net", "", "互联网", "网络之间→互联网"));
        wr8.Examples.Add(new WordRootExample(wr8.Id, "interview", "inter", "view", "", "面试", "相互看→面试"));
        wr8.Examples.Add(new WordRootExample(wr8.Id, "interact", "inter", "act", "", "互动", "相互作用→互动"));
        wr8.Quizzes.Add(new WordRootQuiz(wr8.Id, "international 的意思是什么？", new[] { "互动", "国际的", "互联网", "面试" }, 1));
        roots.Add(wr8);

        // 9: sub-
        var wr9 = new WordRoot(9, "sub-", "Latin", "在下，次", "sub", "sub- 表示「在下面」。submarine（潜水艇）= sub-（在下）+ marine（海）→ 在海下的船。subway（地铁）= sub-（在下）+ way（路）→ 在地下的路。注意：辅音前会变形，如 support = sup- + port。");
        wr9.Examples.Add(new WordRootExample(wr9.Id, "subway", "sub", "way", "", "地铁", "地下通道→地铁"));
        wr9.Examples.Add(new WordRootExample(wr9.Id, "submarine", "sub", "marine", "", "潜水艇", "水下的→潜水艇"));
        wr9.Examples.Add(new WordRootExample(wr9.Id, "subtitle", "sub", "title", "", "字幕", "下方标题→字幕"));
        wr9.Examples.Add(new WordRootExample(wr9.Id, "subconscious", "sub", "conscious", "", "潜意识", "意识之下→潜意识"));
        wr9.Quizzes.Add(new WordRootQuiz(wr9.Id, "subway 的意思是什么？", new[] { "潜水艇", "地铁", "潜意识", "字幕" }, 1));
        roots.Add(wr9);

        // 10: super-
        var wr10 = new WordRoot(10, "super-", "Latin", "在上，超", "super", "super- 来自拉丁语「在上、超过」。superman（超人）、supermarket（超市，比普通市场更大）。记住：super- 强调「超越、胜过」，而 hyper- 强调「过度」。");
        wr10.Examples.Add(new WordRootExample(wr10.Id, "superman", "super", "man", "", "超人", "超级人类→超人"));
        wr10.Examples.Add(new WordRootExample(wr10.Id, "supervise", "super", "vise", "", "监督", "从上看→监督"));
        wr10.Examples.Add(new WordRootExample(wr10.Id, "superior", "super", "ior", "", "优越的", "在上的→优越的"));
        wr10.Examples.Add(new WordRootExample(wr10.Id, "supersonic", "super", "sonic", "", "超音速的", "超过声速→超音速"));
        wr10.Quizzes.Add(new WordRootQuiz(wr10.Id, "superman 的意思是什么？", new[] { "优越的", "监督", "超音速的", "超人" }, 3));
        roots.Add(wr10);

        // 11: pre-
        var wr11 = new WordRoot(11, "pre-", "Latin", "在前，预先", "pre", "pre- 表示「在前、提前」。predict（预测）= pre-（提前）+ dict（说）→ 提前说出来。preview（预览）= pre-（先）+ view（看）→ 先看一眼。这个前缀暗示「时间上靠前」。");
        wr11.Examples.Add(new WordRootExample(wr11.Id, "predict", "pre", "dict", "", "预测", "提前说→预测"));
        wr11.Examples.Add(new WordRootExample(wr11.Id, "prepare", "pre", "pare", "", "准备", "提前准备→准备"));
        wr11.Examples.Add(new WordRootExample(wr11.Id, "preview", "pre", "view", "", "预览", "提前看→预览"));
        wr11.Examples.Add(new WordRootExample(wr11.Id, "prefix", "pre", "fix", "", "前缀", "固定在前→前缀"));
        wr11.Quizzes.Add(new WordRootQuiz(wr11.Id, "predict 的意思是什么？", new[] { "预览", "准备", "预测", "前缀" }, 2));
        roots.Add(wr11);

        // 12: post-
        var wr12 = new WordRoot(12, "post-", "Latin", "在后", "post", "post- 表示「在后」。postpone（推迟）= post-（后）+ pone（放）→ 往后放。postwar（战后的）= post-（后）+ war（战争）→ 战争之后。与 pre- 相对。");
        wr12.Examples.Add(new WordRootExample(wr12.Id, "postwar", "post", "war", "", "战后的", "战争之后→战后"));
        wr12.Examples.Add(new WordRootExample(wr12.Id, "postpone", "post", "pone", "", "推迟", "放到后面→推迟"));
        wr12.Examples.Add(new WordRootExample(wr12.Id, "postgraduate", "post", "graduate", "", "研究生", "毕业后→研究生"));
        wr12.Examples.Add(new WordRootExample(wr12.Id, "postscript", "post", "script", "", "附言", "写在后面→附言"));
        wr12.Quizzes.Add(new WordRootQuiz(wr12.Id, "postwar 的意思是什么？", new[] { "附言", "战后的", "研究生", "推迟" }, 1));
        roots.Add(wr12);

        // 13: re-
        var wr13 = new WordRoot(13, "re-", "Latin", "再，回", "re", "re- 是最常用的前缀之一，表示「再次、回」。return（返回）、review（复习）、recycle（回收利用）。记住：re- 的单词通常表示「重复做某事」或「回到原来的状态」。");
        wr13.Examples.Add(new WordRootExample(wr13.Id, "return", "re", "turn", "", "返回", "转回来→返回"));
        wr13.Examples.Add(new WordRootExample(wr13.Id, "review", "re", "view", "", "复习", "再看→复习"));
        wr13.Examples.Add(new WordRootExample(wr13.Id, "recycle", "re", "cycle", "", "回收", "再循环→回收"));
        wr13.Examples.Add(new WordRootExample(wr13.Id, "repeat", "re", "peat", "", "重复", "再做→重复"));
        wr13.Quizzes.Add(new WordRootQuiz(wr13.Id, "return 的意思是什么？", new[] { "返回", "复习", "重复", "回收" }, 0));
        roots.Add(wr13);

        // 14: mono-
        var wr14 = new WordRoot(14, "mono-", "Greek", "单个，一", "mono", "mono- 来自希腊语「单个、一」。monologue（独白）= mono-（单个）+ logue（说）→ 一个人说 → 独白。monopoly（垄断）= mono-（单个）+ poly（卖）→ 只有一个人卖 → 垄断。monochrome（单色）= mono-（单）+ chrome（色）→ 单一颜色。");
        wr14.Examples.Add(new WordRootExample(wr14.Id, "monopoly", "mono", "poly", "", "垄断", "单独卖→垄断"));
        wr14.Examples.Add(new WordRootExample(wr14.Id, "monologue", "mono", "logue", "", "独白", "一个人说→独白"));
        wr14.Examples.Add(new WordRootExample(wr14.Id, "monotonous", "mono", "ton", "ous", "单调的", "一个声调→单调的"));
        wr14.Examples.Add(new WordRootExample(wr14.Id, "monarch", "mon", "arch", "", "君主", "单独统治→君主"));
        wr14.Quizzes.Add(new WordRootQuiz(wr14.Id, "monopoly 的意思是什么？", new[] { "单调的", "独白", "君主", "垄断" }, 3));
        roots.Add(wr14);

        // 15: bi-
        var wr15 = new WordRoot(15, "bi-", "Latin", "两个，双", "bi", "bi- 来自拉丁语「二、双」。bicycle（自行车）= bi-（双）+ cycle（轮）→ 双轮车。bilingual（双语的）= bi-（双）+ lingual（语言的）。");
        wr15.Examples.Add(new WordRootExample(wr15.Id, "bicycle", "bi", "cycle", "", "自行车", "两个轮→自行车"));
        wr15.Examples.Add(new WordRootExample(wr15.Id, "bilingual", "bi", "lingual", "", "双语的", "两种语言→双语的"));
        wr15.Examples.Add(new WordRootExample(wr15.Id, "biannual", "bi", "annual", "", "一年两次", "两次年度→一年两次"));
        wr15.Examples.Add(new WordRootExample(wr15.Id, "bilateral", "bi", "lateral", "", "双边的", "两边→双边的"));
        wr15.Quizzes.Add(new WordRootQuiz(wr15.Id, "bicycle 的意思是什么？", new[] { "一年两次", "双语的", "双边的", "自行车" }, 3));
        roots.Add(wr15);

        // 16: tri-
        var wr16 = new WordRoot(16, "tri-", "Latin", "三", "tri", "tri- 表示「三」。triangle（三角形）、tricycle（三轮车）。记住：tri- 总是和「3」相关。");
        wr16.Examples.Add(new WordRootExample(wr16.Id, "triangle", "tri", "angle", "", "三角形", "三个角→三角形"));
        wr16.Examples.Add(new WordRootExample(wr16.Id, "tricycle", "tri", "cycle", "", "三轮车", "三个轮→三轮车"));
        wr16.Examples.Add(new WordRootExample(wr16.Id, "triple", "tri", "ple", "", "三倍的", "三重→三倍的"));
        wr16.Examples.Add(new WordRootExample(wr16.Id, "trilogy", "tri", "logy", "", "三部曲", "三个故事→三部曲"));
        wr16.Quizzes.Add(new WordRootQuiz(wr16.Id, "triangle 的意思是什么？", new[] { "三角形", "三倍的", "三部曲", "三轮车" }, 0));
        roots.Add(wr16);

        // 17: multi-
        var wr17 = new WordRoot(17, "multi-", "Latin", "多", "multi", "multi- 表示「多」。multimedia（多媒体）、multinational（跨国的）。记住：multi- 强调「很多、多种」。");
        wr17.Examples.Add(new WordRootExample(wr17.Id, "multimedia", "multi", "media", "", "多媒体", "多个媒介→多媒体"));
        wr17.Examples.Add(new WordRootExample(wr17.Id, "multinational", "multi", "national", "", "跨国的", "多个国家→跨国的"));
        wr17.Examples.Add(new WordRootExample(wr17.Id, "multiple", "multi", "ple", "", "多重的", "多个→多重的"));
        wr17.Examples.Add(new WordRootExample(wr17.Id, "multiply", "multi", "ply", "", "乘；繁殖", "变多→乘"));
        wr17.Quizzes.Add(new WordRootQuiz(wr17.Id, "multimedia 的意思是什么？", new[] { "多重的", "跨国的", "多媒体", "乘；繁殖" }, 2));
        roots.Add(wr17);

        // 18: micro-
        var wr18 = new WordRoot(18, "micro-", "Greek", "微，小", "micro", "micro- 来自希腊语「小」。microscope（显微镜）= micro-（小）+ scope（看）→ 看微小东西的工具。microphone（麦克风）= micro-（小）+ phone（声音）→ 小声音也能听到。");
        wr18.Examples.Add(new WordRootExample(wr18.Id, "microscope", "micro", "scope", "", "显微镜", "看小东西→显微镜"));
        wr18.Examples.Add(new WordRootExample(wr18.Id, "microphone", "micro", "phone", "", "麦克风", "小声音→麦克风"));
        wr18.Examples.Add(new WordRootExample(wr18.Id, "microwave", "micro", "wave", "", "微波", "小波→微波"));
        wr18.Examples.Add(new WordRootExample(wr18.Id, "microorganism", "micro", "organism", "", "微生物", "小生物→微生物"));
        wr18.Quizzes.Add(new WordRootQuiz(wr18.Id, "microscope 的意思是什么？", new[] { "微波", "微生物", "麦克风", "显微镜" }, 3));
        roots.Add(wr18);

        // 19: macro-
        var wr19 = new WordRoot(19, "macro-", "Greek", "大，宏观", "macro", "macro- 表示「大、宏观」（与 micro- 相对）。macroeconomics（宏观经济学）= macro-（大）+ economics（经济学）。macroscopic（肉眼可见的）= macro-（大）+ scopic（看）。");
        wr19.Examples.Add(new WordRootExample(wr19.Id, "macroeconomics", "macro", "economics", "", "宏观经济学", "大经济→宏观经济"));
        wr19.Examples.Add(new WordRootExample(wr19.Id, "macroscopic", "macro", "scopic", "", "宏观的", "大范围看→宏观"));
        wr19.Examples.Add(new WordRootExample(wr19.Id, "macrocosm", "macro", "cosm", "", "宇宙", "大世界→宇宙"));
        wr19.Examples.Add(new WordRootExample(wr19.Id, "macrobiotic", "macro", "biotic", "", "长寿的", "大生命→长寿"));
        wr19.Quizzes.Add(new WordRootQuiz(wr19.Id, "macroeconomics 的意思是什么？", new[] { "宏观经济学", "宇宙", "宏观的", "长寿的" }, 0));
        roots.Add(wr19);

        // 20: mini-
        var wr20 = new WordRoot(20, "mini-", "Latin", "小", "mini", "mini- 表示「小、迷你」。minimum（最小值）、miniature（微型）。记住：mini- 强调「小型化」，如 miniskirt（迷你裙）。");
        wr20.Examples.Add(new WordRootExample(wr20.Id, "minimum", "mini", "mum", "", "最小值", "最小→最小值"));
        wr20.Examples.Add(new WordRootExample(wr20.Id, "miniature", "mini", "ature", "", "微型的", "小的→微型的"));
        wr20.Examples.Add(new WordRootExample(wr20.Id, "minimize", "mini", "mize", "", "最小化", "使变小→最小化"));
        wr20.Examples.Add(new WordRootExample(wr20.Id, "miniskirt", "mini", "skirt", "", "迷你裙", "小裙子→迷你裙"));
        wr20.Quizzes.Add(new WordRootQuiz(wr20.Id, "minimum 的意思是什么？", new[] { "最小化", "迷你裙", "微型的", "最小值" }, 3));
        roots.Add(wr20);

        // 21: auto-
        var wr21 = new WordRoot(21, "auto-", "Greek", "自己，自动", "auto", "auto- 来自希腊语「自己」。automobile（汽车）= auto-（自己）+ mobile（移动）→ 自己会动的车。autobiography（自传）= auto-（自己）+ bio（生命）+ graphy（写）→ 写自己的人生。");
        wr21.Examples.Add(new WordRootExample(wr21.Id, "automatic", "auto", "matic", "", "自动的", "自己动→自动的"));
        wr21.Examples.Add(new WordRootExample(wr21.Id, "autobiography", "auto", "biography", "", "自传", "自己写传→自传"));
        wr21.Examples.Add(new WordRootExample(wr21.Id, "autonomous", "auto", "nomous", "", "自治的", "自己管理→自治的"));
        wr21.Examples.Add(new WordRootExample(wr21.Id, "automobile", "auto", "mobile", "", "汽车", "自己动→汽车"));
        wr21.Quizzes.Add(new WordRootQuiz(wr21.Id, "automatic 的意思是什么？", new[] { "自治的", "自动的", "汽车", "自传" }, 1));
        roots.Add(wr21);

        // 22: co-/com-/con-
        var wr22 = new WordRoot(22, "co-/com-/con-", "Latin", "共同", "co", "co-/com-/con- 表示「共同、一起」。cooperate（合作）= co-（共同）+ operate（操作）→ 一起做事。company（公司）= com-（一起）+ pan（面包）+ -y → 一起吃面包的人 → 伙伴 → 公司。");
        wr22.Examples.Add(new WordRootExample(wr22.Id, "cooperation", "co", "operation", "", "合作", "共同操作→合作"));
        wr22.Examples.Add(new WordRootExample(wr22.Id, "combine", "com", "bine", "", "结合", "放在一起→结合"));
        wr22.Examples.Add(new WordRootExample(wr22.Id, "connect", "con", "nect", "", "连接", "绑在一起→连接"));
        wr22.Examples.Add(new WordRootExample(wr22.Id, "collect", "col", "lect", "", "收集", "一起选→收集"));
        wr22.Quizzes.Add(new WordRootQuiz(wr22.Id, "cooperation 的意思是什么？", new[] { "收集", "结合", "连接", "合作" }, 3));
        roots.Add(wr22);

        // 23: tele-
        var wr23 = new WordRoot(23, "tele-", "Greek", "远", "tele", "tele- 来自希腊语「远」。television（电视）= tele-（远）+ vision（看）→ 看远处的东西。telephone（电话）= tele-（远）+ phone（声音）→ 远距离传声。");
        wr23.Examples.Add(new WordRootExample(wr23.Id, "telephone", "tele", "phone", "", "电话", "远距离声音→电话"));
        wr23.Examples.Add(new WordRootExample(wr23.Id, "television", "tele", "vision", "", "电视", "远距离看→电视"));
        wr23.Examples.Add(new WordRootExample(wr23.Id, "telescope", "tele", "scope", "", "望远镜", "看远处→望远镜"));
        wr23.Examples.Add(new WordRootExample(wr23.Id, "telegram", "tele", "gram", "", "电报", "远距离写→电报"));
        wr23.Quizzes.Add(new WordRootQuiz(wr23.Id, "telephone 的意思是什么？", new[] { "望远镜", "电报", "电视", "电话" }, 3));
        roots.Add(wr23);

        // 24: trans-
        var wr24 = new WordRoot(24, "trans-", "Latin", "穿过，转换", "trans", "trans- 表示「穿过、转变」。transport（运输）= trans-（穿过）+ port（运）→ 运送穿越。translate（翻译）= trans-（转）+ late（搬运）→ 把意思从一种语言转到另一种。");
        wr24.Examples.Add(new WordRootExample(wr24.Id, "transport", "trans", "port", "", "运输", "带过去→运输"));
        wr24.Examples.Add(new WordRootExample(wr24.Id, "translate", "trans", "late", "", "翻译", "转换语言→翻译"));
        wr24.Examples.Add(new WordRootExample(wr24.Id, "transfer", "trans", "fer", "", "转移", "带过去→转移"));
        wr24.Examples.Add(new WordRootExample(wr24.Id, "transform", "trans", "form", "", "变形", "改变形式→变形"));
        wr24.Quizzes.Add(new WordRootQuiz(wr24.Id, "transport 的意思是什么？", new[] { "转移", "变形", "翻译", "运输" }, 3));
        roots.Add(wr24);

        // 25: uni-
        var wr25 = new WordRoot(25, "uni-", "Latin", "单一，统一", "uni", "uni- 来自拉丁语「一」。uniform（制服）= uni-（统一）+ form（形式）→ 统一的形式。unique（独特的）= uni-（一）+ -que → 唯一的 → 独特的。universe（宇宙）= uni-（一）+ verse（转）→ 作为一个整体旋转的东西。");
        wr25.Examples.Add(new WordRootExample(wr25.Id, "uniform", "uni", "form", "", "制服；统一的", "一种形式→制服"));
        wr25.Examples.Add(new WordRootExample(wr25.Id, "unique", "uni", "que", "", "独特的", "唯一的→独特的"));
        wr25.Examples.Add(new WordRootExample(wr25.Id, "unite", "uni", "te", "", "联合", "成为一体→联合"));
        wr25.Examples.Add(new WordRootExample(wr25.Id, "universe", "uni", "verse", "", "宇宙", "统一的世界→宇宙"));
        wr25.Quizzes.Add(new WordRootQuiz(wr25.Id, "uniform 的意思是什么？", new[] { "联合", "独特的", "宇宙", "制服；统一的" }, 3));
        roots.Add(wr25);

        // 26: spect/spec
        var wr26 = new WordRoot(26, "spect/spec", "Latin", "看", "spect", "spect/spec 来自拉丁语「看」。respect（尊重）= re-（再）+ spect（看）→ 再看一眼 → 重视。inspect（检查）= in-（向内）+ spect（看）→ 仔细看里面。这是最重要的词根之一，衍生出50+单词。");
        wr26.Examples.Add(new WordRootExample(wr26.Id, "inspect", "in", "spect", "", "检查", "向内看→检查"));
        wr26.Examples.Add(new WordRootExample(wr26.Id, "respect", "re", "spect", "", "尊重", "再看→尊重"));
        wr26.Examples.Add(new WordRootExample(wr26.Id, "prospect", "pro", "spect", "", "前景", "向前看→前景"));
        wr26.Examples.Add(new WordRootExample(wr26.Id, "spectator", "", "spect", "ator", "观众", "看的人→观众"));
        wr26.Examples.Add(new WordRootExample(wr26.Id, "aspect", "a", "spect", "", "方面", "朝向看→方面"));
        wr26.Quizzes.Add(new WordRootQuiz(wr26.Id, "inspect 的意思是什么？", new[] { "检查", "尊重", "前景", "观众" }, 0));
        roots.Add(wr26);

        // 27: vis/vid
        var wr27 = new WordRoot(27, "vis/vid", "Latin", "看见", "vis", "vis/vid 也表示「看见」，但更强调「视觉」。television = tele-（远）+ vis（看）+ -ion → 看远处的东西。video（视频）直接来自拉丁语「我看到」。");
        wr27.Examples.Add(new WordRootExample(wr27.Id, "visible", "", "vis", "ible", "可见的", "能看见→可见的"));
        wr27.Examples.Add(new WordRootExample(wr27.Id, "invisible", "in", "vis", "ible", "看不见的", "不能看见→隐形的"));
        wr27.Examples.Add(new WordRootExample(wr27.Id, "supervise", "super", "vis", "e", "监督", "从上面看→监督"));
        wr27.Examples.Add(new WordRootExample(wr27.Id, "television", "tele", "vis", "ion", "电视", "远距离看→电视"));
        wr27.Examples.Add(new WordRootExample(wr27.Id, "video", "", "vid", "eo", "视频", "看的东西→视频"));
        wr27.Quizzes.Add(new WordRootQuiz(wr27.Id, "visible 的意思是什么？", new[] { "监督", "可见的", "看不见的", "电视" }, 1));
        roots.Add(wr27);

        // 28: aud/audit
        var wr28 = new WordRoot(28, "aud/audit", "Latin", "听", "aud", "aud/audit 表示「听」。audio（音频）、audience（观众）= aud（听）+ -ience（人）→ 听的人。auditorium（礼堂）= audit（听）+ -orium（地方）→ 听的地方。");
        wr28.Examples.Add(new WordRootExample(wr28.Id, "audio", "", "aud", "io", "音频", "用来听→音频"));
        wr28.Examples.Add(new WordRootExample(wr28.Id, "audience", "", "aud", "ience", "观众", "听的人→观众"));
        wr28.Examples.Add(new WordRootExample(wr28.Id, "audible", "", "aud", "ible", "听得见的", "能听见→听得见的"));
        wr28.Examples.Add(new WordRootExample(wr28.Id, "auditorium", "", "aud", "itorium", "礼堂", "听的地方→礼堂"));
        wr28.Examples.Add(new WordRootExample(wr28.Id, "audit", "", "audit", "", "审计", "听取报告→审计"));
        wr28.Quizzes.Add(new WordRootQuiz(wr28.Id, "audio 的意思是什么？", new[] { "音频", "听得见的", "礼堂", "观众" }, 0));
        roots.Add(wr28);

        // 29: dict
        var wr29 = new WordRoot(29, "dict", "Latin", "说", "dict", "dict 表示「说」。dictionary（字典）= dict（说）+ -ion（名词）+ -ary（的）→ 说明词语的书。predict（预测）= pre-（提前）+ dict（说）→ 提前说出来。");
        wr29.Examples.Add(new WordRootExample(wr29.Id, "predict", "pre", "dict", "", "预测", "提前说→预测"));
        wr29.Examples.Add(new WordRootExample(wr29.Id, "dictionary", "", "dict", "ionary", "字典", "说话的书→字典"));
        wr29.Examples.Add(new WordRootExample(wr29.Id, "contradict", "contra", "dict", "", "反驳", "说相反的→反驳"));
        wr29.Examples.Add(new WordRootExample(wr29.Id, "verdict", "ver", "dict", "", "裁决", "说真话→裁决"));
        wr29.Examples.Add(new WordRootExample(wr29.Id, "dictate", "", "dict", "ate", "口述", "说出来→口述"));
        wr29.Quizzes.Add(new WordRootQuiz(wr29.Id, "predict 的意思是什么？", new[] { "字典", "反驳", "裁决", "预测" }, 3));
        roots.Add(wr29);

        // 30: scrib/script
        var wr30 = new WordRoot(30, "scrib/script", "Latin", "写", "scrib", "scrib/script 表示「写」。describe（描述）= de-（完全）+ scrib（写）→ 详细写出来。manuscript（手稿）= manu-（手）+ script（写）→ 手写的东西。");
        wr30.Examples.Add(new WordRootExample(wr30.Id, "describe", "de", "scrib", "e", "描述", "写下来→描述"));
        wr30.Examples.Add(new WordRootExample(wr30.Id, "prescribe", "pre", "scrib", "e", "开处方", "提前写→开处方"));
        wr30.Examples.Add(new WordRootExample(wr30.Id, "subscribe", "sub", "scrib", "e", "订阅", "在下面签名→订阅"));
        wr30.Examples.Add(new WordRootExample(wr30.Id, "manuscript", "manu", "script", "", "手稿", "手写的→手稿"));
        wr30.Examples.Add(new WordRootExample(wr30.Id, "transcript", "trans", "script", "", "抄本", "抄写过来→抄本"));
        wr30.Quizzes.Add(new WordRootQuiz(wr30.Id, "describe 的意思是什么？", new[] { "手稿", "订阅", "开处方", "描述" }, 3));
        roots.Add(wr30);

        // 31: graph
        var wr31 = new WordRoot(31, "graph", "Greek", "写，画", "graph", "graph 表示「写、画、记录」。photograph（照片）= photo-（光）+ graph（画）→ 用光画出来的图。biography（传记）= bio-（生命）+ graph（写）→ 写人生的书。");
        wr31.Examples.Add(new WordRootExample(wr31.Id, "photograph", "photo", "graph", "", "照片", "用光画→照片"));
        wr31.Examples.Add(new WordRootExample(wr31.Id, "paragraph", "para", "graph", "", "段落", "写在旁边→段落"));
        wr31.Examples.Add(new WordRootExample(wr31.Id, "autograph", "auto", "graph", "", "亲笔签名", "自己写→签名"));
        wr31.Examples.Add(new WordRootExample(wr31.Id, "biography", "bio", "graph", "y", "传记", "写生平→传记"));
        wr31.Examples.Add(new WordRootExample(wr31.Id, "graphic", "", "graph", "ic", "图形的", "画的→图形的"));
        wr31.Quizzes.Add(new WordRootQuiz(wr31.Id, "photograph 的意思是什么？", new[] { "传记", "段落", "照片", "亲笔签名" }, 2));
        roots.Add(wr31);

        // 32: port
        var wr32 = new WordRoot(32, "port", "Latin", "拿，带", "port", "port 表示「拿、带、运」。export（出口）= ex-（向外）+ port（运）→ 运出去。portable（便携的）= port（带）+ -able（可...的）→ 可以带着走的。");
        wr32.Examples.Add(new WordRootExample(wr32.Id, "transport", "trans", "port", "", "运输", "带过去→运输"));
        wr32.Examples.Add(new WordRootExample(wr32.Id, "export", "ex", "port", "", "出口", "带出去→出口"));
        wr32.Examples.Add(new WordRootExample(wr32.Id, "import", "im", "port", "", "进口", "带进来→进口"));
        wr32.Examples.Add(new WordRootExample(wr32.Id, "support", "sup", "port", "", "支持", "从下托起→支持"));
        wr32.Examples.Add(new WordRootExample(wr32.Id, "portable", "", "port", "able", "便携的", "能带的→便携的"));
        wr32.Quizzes.Add(new WordRootQuiz(wr32.Id, "transport 的意思是什么？", new[] { "运输", "出口", "进口", "支持" }, 0));
        roots.Add(wr32);

        // 33: duct/duc
        var wr33 = new WordRoot(33, "duct/duc", "Latin", "引导", "duct", "duct/duc 表示「引导、带领」。conduct（引导）、educate（教育）= e-（向外）+ duc（引）+ -ate（使）→ 把知识引出来 → 教育。");
        wr33.Examples.Add(new WordRootExample(wr33.Id, "conduct", "con", "duct", "", "引导", "一起引导→指挥"));
        wr33.Examples.Add(new WordRootExample(wr33.Id, "produce", "pro", "duce", "", "生产", "向前引→生产"));
        wr33.Examples.Add(new WordRootExample(wr33.Id, "reduce", "re", "duce", "", "减少", "向后引→减少"));
        wr33.Examples.Add(new WordRootExample(wr33.Id, "educate", "e", "duc", "ate", "教育", "引出来→教育"));
        wr33.Examples.Add(new WordRootExample(wr33.Id, "introduce", "intro", "duce", "", "介绍", "引进来→介绍"));
        wr33.Quizzes.Add(new WordRootQuiz(wr33.Id, "conduct 的意思是什么？", new[] { "教育", "生产", "引导", "减少" }, 2));
        roots.Add(wr33);

        // 34: fer
        var wr34 = new WordRoot(34, "fer", "Latin", "带，拿", "fer", "fer 表示「带、拿」。transfer（转移）= trans-（穿过）+ fer（带）→ 带过去。refer（提到）= re-（回）+ fer（带）→ 带回来说 → 提到。");
        wr34.Examples.Add(new WordRootExample(wr34.Id, "transfer", "trans", "fer", "", "转移", "带过去→转移"));
        wr34.Examples.Add(new WordRootExample(wr34.Id, "refer", "re", "fer", "", "参考", "带回来→参考"));
        wr34.Examples.Add(new WordRootExample(wr34.Id, "prefer", "pre", "fer", "", "偏爱", "带到前面→偏爱"));
        wr34.Examples.Add(new WordRootExample(wr34.Id, "differ", "dif", "fer", "", "不同", "带开→不同"));
        wr34.Examples.Add(new WordRootExample(wr34.Id, "offer", "of", "fer", "", "提供", "带向→提供"));
        wr34.Quizzes.Add(new WordRootQuiz(wr34.Id, "transfer 的意思是什么？", new[] { "转移", "偏爱", "参考", "不同" }, 0));
        roots.Add(wr34);

        // 35: mit/miss
        var wr35 = new WordRoot(35, "mit/miss", "Latin", "送，放", "mit", "mit/miss 表示「送、放」。submit（提交）= sub-（向下）+ mit（送）→ 向下送 → 提交。mission（任务）= miss（送）+ -ion → 被派遣去做的事 → 任务。");
        wr35.Examples.Add(new WordRootExample(wr35.Id, "admit", "ad", "mit", "", "承认", "向...送→承认"));
        wr35.Examples.Add(new WordRootExample(wr35.Id, "commit", "com", "mit", "", "承诺", "一起送→承诺"));
        wr35.Examples.Add(new WordRootExample(wr35.Id, "dismiss", "dis", "miss", "", "解散", "送开→解散"));
        wr35.Examples.Add(new WordRootExample(wr35.Id, "permit", "per", "mit", "", "允许", "让通过→允许"));
        wr35.Examples.Add(new WordRootExample(wr35.Id, "transmit", "trans", "mit", "", "传送", "送过去→传送"));
        wr35.Quizzes.Add(new WordRootQuiz(wr35.Id, "admit 的意思是什么？", new[] { "解散", "承认", "承诺", "允许" }, 1));
        roots.Add(wr35);

        // 36: pos/posit
        var wr36 = new WordRoot(36, "pos/posit", "Latin", "放", "pos", "pos/pon 表示「放置」。compose（组成）= com-（一起）+ pos（放）→ 放在一起 → 组成。postpone（推迟）= post-（后）+ pon（放）→ 往后放 → 推迟。");
        wr36.Examples.Add(new WordRootExample(wr36.Id, "position", "", "posit", "ion", "位置", "放的地方→位置"));
        wr36.Examples.Add(new WordRootExample(wr36.Id, "compose", "com", "pose", "", "组成", "放在一起→组成"));
        wr36.Examples.Add(new WordRootExample(wr36.Id, "dispose", "dis", "pose", "", "处理", "分开放→处理"));
        wr36.Examples.Add(new WordRootExample(wr36.Id, "expose", "ex", "pose", "", "暴露", "放出来→暴露"));
        wr36.Examples.Add(new WordRootExample(wr36.Id, "oppose", "op", "pose", "", "反对", "对着放→反对"));
        wr36.Quizzes.Add(new WordRootQuiz(wr36.Id, "position 的意思是什么？", new[] { "组成", "暴露", "处理", "位置" }, 3));
        roots.Add(wr36);

        // 37: st/sta/stat
        var wr37 = new WordRoot(37, "st/sta/stat", "Latin", "站", "st", "st/sta/stat 表示「站、立」。stand（站立）来自同源。station（车站）= stat（站）+ -ion → 站的地方。stable（稳定的）= sta（站）+ -ble（能...的）→ 能站稳的 → 稳定的。statue（雕像）= stat（站）+ -ue → 站着的东西。");
        wr37.Examples.Add(new WordRootExample(wr37.Id, "stand", "", "st", "and", "站", "站立"));
        wr37.Examples.Add(new WordRootExample(wr37.Id, "stable", "", "sta", "ble", "稳定的", "能站→稳定的"));
        wr37.Examples.Add(new WordRootExample(wr37.Id, "statue", "", "stat", "ue", "雕像", "站着的→雕像"));
        wr37.Examples.Add(new WordRootExample(wr37.Id, "station", "", "stat", "ion", "车站", "站的地方→车站"));
        wr37.Examples.Add(new WordRootExample(wr37.Id, "status", "", "stat", "us", "状态", "站的样子→状态"));
        wr37.Quizzes.Add(new WordRootQuiz(wr37.Id, "stand 的意思是什么？", new[] { "稳定的", "车站", "雕像", "站" }, 3));
        roots.Add(wr37);

        // 38: ject
        var wr38 = new WordRoot(38, "ject", "Latin", "投，掷", "ject", "ject 表示「扔、投」。project（项目）= pro-（向前）+ ject（扔）→ 向前扔的计划。reject（拒绝）= re-（回）+ ject（扔）→ 扔回去 → 拒绝。inject（注射）= in-（进入）+ ject（扔）→ 扔进去 → 注射。");
        wr38.Examples.Add(new WordRootExample(wr38.Id, "project", "pro", "ject", "", "投射", "向前掷→投射"));
        wr38.Examples.Add(new WordRootExample(wr38.Id, "reject", "re", "ject", "", "拒绝", "往回扔→拒绝"));
        wr38.Examples.Add(new WordRootExample(wr38.Id, "inject", "in", "ject", "", "注射", "扔进去→注射"));
        wr38.Examples.Add(new WordRootExample(wr38.Id, "object", "ob", "ject", "", "反对", "对着扔→反对"));
        wr38.Examples.Add(new WordRootExample(wr38.Id, "subject", "sub", "ject", "", "主题", "扔在下面→主题"));
        wr38.Quizzes.Add(new WordRootQuiz(wr38.Id, "project 的意思是什么？", new[] { "注射", "投射", "拒绝", "反对" }, 1));
        roots.Add(wr38);

        // 39: ment
        var wr39 = new WordRoot(39, "ment", "Latin", "心智", "ment", "ment 表示「心智、思考」。mental（精神的）、mention（提及）= ment（心智）+ -ion → 心里想到 → 提及。comment（评论）= com-（一起）+ ment（心智）+ -t → 一起思考 → 评论。");
        wr39.Examples.Add(new WordRootExample(wr39.Id, "mental", "", "ment", "al", "精神的", "心智的→精神的"));
        wr39.Examples.Add(new WordRootExample(wr39.Id, "comment", "com", "ment", "", "评论", "用心想→评论"));
        wr39.Examples.Add(new WordRootExample(wr39.Id, "mention", "", "ment", "ion", "提及", "心里想到→提及"));
        wr39.Examples.Add(new WordRootExample(wr39.Id, "sentiment", "sent", "ment", "", "情感", "感受→情感"));
        wr39.Examples.Add(new WordRootExample(wr39.Id, "monument", "monu", "ment", "", "纪念碑", "提醒的东西→纪念碑"));
        wr39.Quizzes.Add(new WordRootQuiz(wr39.Id, "mental 的意思是什么？", new[] { "评论", "情感", "精神的", "提及" }, 2));
        roots.Add(wr39);

        // 40: sens/sent
        var wr40 = new WordRoot(40, "sens/sent", "Latin", "感觉", "sens", "sens/sent 表示「感觉」。sense（感觉）、sensitive（敏感的）= sens（感觉）+ -itive → 有感觉的 → 敏感的。sentiment（情感）= sent（感觉）+ -iment → 感受 → 情感。");
        wr40.Examples.Add(new WordRootExample(wr40.Id, "sense", "", "sens", "e", "感觉", "感觉"));
        wr40.Examples.Add(new WordRootExample(wr40.Id, "sensitive", "", "sens", "itive", "敏感的", "能感觉→敏感的"));
        wr40.Examples.Add(new WordRootExample(wr40.Id, "consent", "con", "sent", "", "同意", "一起感觉→同意"));
        wr40.Examples.Add(new WordRootExample(wr40.Id, "resent", "re", "sent", "", "愤恨", "反感→愤恨"));
        wr40.Examples.Add(new WordRootExample(wr40.Id, "sentiment", "", "sent", "iment", "情感", "感觉→情感"));
        wr40.Quizzes.Add(new WordRootQuiz(wr40.Id, "sense 的意思是什么？", new[] { "敏感的", "愤恨", "感觉", "同意" }, 2));
        roots.Add(wr40);

        // 41: mem/memor
        var wr41 = new WordRoot(41, "mem/memor", "Latin", "记忆", "mem", "mem/memor 表示「记忆」。memory（记忆）、remember（记得）= re-（再）+ member（记忆）→ 再次记起。memorial（纪念碑）= memor（记忆）+ -ial → 记忆的东西 → 纪念碑。");
        wr41.Examples.Add(new WordRootExample(wr41.Id, "memory", "", "memor", "y", "记忆", "记忆"));
        wr41.Examples.Add(new WordRootExample(wr41.Id, "remember", "re", "member", "", "记得", "再次想起→记得"));
        wr41.Examples.Add(new WordRootExample(wr41.Id, "memorize", "", "memor", "ize", "记住", "使记住→记住"));
        wr41.Examples.Add(new WordRootExample(wr41.Id, "memorial", "", "memor", "ial", "纪念的", "记忆的→纪念的"));
        wr41.Examples.Add(new WordRootExample(wr41.Id, "commemorate", "com", "memor", "ate", "纪念", "共同记忆→纪念"));
        wr41.Quizzes.Add(new WordRootQuiz(wr41.Id, "memory 的意思是什么？", new[] { "记住", "纪念的", "记得", "记忆" }, 3));
        roots.Add(wr41);

        // 42: bio
        var wr42 = new WordRoot(42, "bio", "Greek", "生命", "bio", "bio 表示「生命」（希腊语）。biology（生物学）= bio（生命）+ log（学）+ -y → 研究生命的学问。biography（传记）= bio（生命）+ graph（写）+ -y → 写人生的书。antibiotic（抗生素）= anti-（对抗）+ bio（生命）+ -tic → 对抗生命的（细菌）。");
        wr42.Examples.Add(new WordRootExample(wr42.Id, "biology", "", "bio", "logy", "生物学", "生命学→生物学"));
        wr42.Examples.Add(new WordRootExample(wr42.Id, "biography", "", "bio", "graphy", "传记", "写生命→传记"));
        wr42.Examples.Add(new WordRootExample(wr42.Id, "antibiotic", "anti", "bio", "tic", "抗生素", "对抗微生物→抗生素"));
        wr42.Examples.Add(new WordRootExample(wr42.Id, "symbiosis", "sym", "bio", "sis", "共生", "一起生活→共生"));
        wr42.Examples.Add(new WordRootExample(wr42.Id, "biochemistry", "", "bio", "chemistry", "生物化学", "生命化学→生物化学"));
        wr42.Quizzes.Add(new WordRootQuiz(wr42.Id, "biology 的意思是什么？", new[] { "生物学", "共生", "抗生素", "传记" }, 0));
        roots.Add(wr42);

        // 43: vit/viv
        var wr43 = new WordRoot(43, "vit/viv", "Latin", "生命，活", "vit", "vit/viv 表示「生命、活」。vital（至关重要的）= vit（生命）+ -al → 生命的 → 重要的。survive（生存）= sur-（超过）+ viv（活）+ -e → 活过来 → 生存。revive（复活）= re-（再）+ viv（活）+ -e → 再活过来 → 复活。");
        wr43.Examples.Add(new WordRootExample(wr43.Id, "vital", "", "vit", "al", "至关重要的", "生命的→重要的"));
        wr43.Examples.Add(new WordRootExample(wr43.Id, "vitamin", "", "vit", "amin", "维生素", "生命素→维生素"));
        wr43.Examples.Add(new WordRootExample(wr43.Id, "survive", "sur", "viv", "e", "生存", "活下来→生存"));
        wr43.Examples.Add(new WordRootExample(wr43.Id, "revive", "re", "viv", "e", "复活", "再次活→复活"));
        wr43.Examples.Add(new WordRootExample(wr43.Id, "vivid", "", "viv", "id", "生动的", "活的→生动的"));
        wr43.Quizzes.Add(new WordRootQuiz(wr43.Id, "vital 的意思是什么？", new[] { "至关重要的", "复活", "生存", "维生素" }, 0));
        roots.Add(wr43);

        // 44: anim
        var wr44 = new WordRoot(44, "anim", "Latin", "生命，心", "anim", "anim 表示「生命、心」。animal（动物）= anim（生命）+ -al → 有生命的东西。animate（使有生气）= anim（生命）+ -ate → 使有生命。unanimous（一致的）= un-（一）+ anim（心）+ -ous → 一条心的 → 一致的。");
        wr44.Examples.Add(new WordRootExample(wr44.Id, "animal", "", "anim", "al", "动物", "有生命的→动物"));
        wr44.Examples.Add(new WordRootExample(wr44.Id, "animate", "", "anim", "ate", "使有生气", "使活→使有生气"));
        wr44.Examples.Add(new WordRootExample(wr44.Id, "unanimous", "un", "anim", "ous", "一致的", "一个心→一致的"));
        wr44.Examples.Add(new WordRootExample(wr44.Id, "magnanimous", "magn", "anim", "ous", "宽宏大量的", "大心的→宽宏的"));
        wr44.Examples.Add(new WordRootExample(wr44.Id, "animation", "", "anim", "ation", "动画", "使活的→动画"));
        wr44.Quizzes.Add(new WordRootQuiz(wr44.Id, "animal 的意思是什么？", new[] { "一致的", "宽宏大量的", "动物", "使有生气" }, 2));
        roots.Add(wr44);

        // 45: mort
        var wr45 = new WordRoot(45, "mort", "Latin", "死", "mort", "mort 表示「死」。mortal（凡人）= mort（死）+ -al → 会死的人 → 凡人。immortal（不朽的）= im-（不）+ mort（死）+ -al → 不会死的 → 不朽的。mortgage（抵押贷款）= mort（死）+ gage（抵押）→ 死了也要还的贷款。");
        wr45.Examples.Add(new WordRootExample(wr45.Id, "mortal", "", "mort", "al", "会死的", "死的→凡人的"));
        wr45.Examples.Add(new WordRootExample(wr45.Id, "immortal", "im", "mort", "al", "不朽的", "不会死→不朽的"));
        wr45.Examples.Add(new WordRootExample(wr45.Id, "mortgage", "", "mort", "gage", "抵押", "死的保证→抵押"));
        wr45.Examples.Add(new WordRootExample(wr45.Id, "mortuary", "", "mort", "uary", "太平间", "死的地方→太平间"));
        wr45.Examples.Add(new WordRootExample(wr45.Id, "mortify", "", "mort", "ify", "使屈辱", "使如死→屈辱"));
        wr45.Quizzes.Add(new WordRootQuiz(wr45.Id, "mortal 的意思是什么？", new[] { "会死的", "不朽的", "抵押", "太平间" }, 0));
        roots.Add(wr45);

        // 46: chron
        var wr46 = new WordRoot(46, "chron", "Greek", "时间", "chron", "chron 表示「时间」（希腊语）。chronic（慢性的）= chron（时间）+ -ic → 持续很长时间的 → 慢性的。chronological（按时间顺序的）= chron（时间）+ log（学）+ -ical → 时间顺序的。synchronize（同步）= syn-（一起）+ chron（时间）+ -ize → 时间一起 → 同步。");
        wr46.Examples.Add(new WordRootExample(wr46.Id, "chronic", "", "chron", "ic", "慢性的", "时间长的→慢性的"));
        wr46.Examples.Add(new WordRootExample(wr46.Id, "chronology", "", "chron", "ology", "年代学", "时间学→年代学"));
        wr46.Examples.Add(new WordRootExample(wr46.Id, "synchronize", "syn", "chron", "ize", "同步", "同时间→同步"));
        wr46.Examples.Add(new WordRootExample(wr46.Id, "anachronism", "ana", "chron", "ism", "时代错误", "错误时间→时代错误"));
        wr46.Examples.Add(new WordRootExample(wr46.Id, "chronicle", "", "chron", "icle", "编年史", "时间记录→编年史"));
        wr46.Quizzes.Add(new WordRootQuiz(wr46.Id, "chronic 的意思是什么？", new[] { "时代错误", "慢性的", "同步", "年代学" }, 1));
        roots.Add(wr46);

        // 47: temp/tempor
        var wr47 = new WordRoot(47, "temp/tempor", "Latin", "时间", "temp", "temp/tempor 表示「时间」（拉丁语）。temporary（临时的）= tempor（时间）+ -ary → 时间性的 → 临时的。contemporary（当代的）= con-（一起）+ tempor（时间）+ -ary → 同一时间的 → 当代的。");
        wr47.Examples.Add(new WordRootExample(wr47.Id, "temporary", "", "tempor", "ary", "临时的", "时间的→临时的"));
        wr47.Examples.Add(new WordRootExample(wr47.Id, "contemporary", "con", "tempor", "ary", "当代的", "同时间→当代的"));
        wr47.Examples.Add(new WordRootExample(wr47.Id, "tempo", "", "tempo", "", "节奏", "时间→节奏"));
        wr47.Examples.Add(new WordRootExample(wr47.Id, "temporal", "", "tempor", "al", "时间的", "时间的"));
        wr47.Examples.Add(new WordRootExample(wr47.Id, "temper", "", "temper", "", "脾气", "时间状态→脾气"));
        wr47.Quizzes.Add(new WordRootQuiz(wr47.Id, "temporary 的意思是什么？", new[] { "临时的", "节奏", "时间的", "当代的" }, 0));
        roots.Add(wr47);

        // 48: ann/enn
        var wr48 = new WordRoot(48, "ann/enn", "Latin", "年", "ann", "ann/enn 表示「年」。annual（年度的）= ann（年）+ -ual → 每年的。anniversary（周年纪念）= anni-（年）+ vers（转）+ -ary → 年份转一圈 → 周年。millennium（千年）= mill-（千）+ enn（年）+ -ium → 一千年。");
        wr48.Examples.Add(new WordRootExample(wr48.Id, "annual", "", "ann", "ual", "每年的", "年的→每年的"));
        wr48.Examples.Add(new WordRootExample(wr48.Id, "anniversary", "", "ann", "iversary", "周年纪念", "年的转折→周年"));
        wr48.Examples.Add(new WordRootExample(wr48.Id, "annuity", "", "ann", "uity", "年金", "年的钱→年金"));
        wr48.Examples.Add(new WordRootExample(wr48.Id, "perennial", "per", "enn", "ial", "长期的", "穿过年→长期的"));
        wr48.Examples.Add(new WordRootExample(wr48.Id, "biennial", "bi", "enn", "ial", "两年一次", "两年→两年一次"));
        wr48.Quizzes.Add(new WordRootQuiz(wr48.Id, "annual 的意思是什么？", new[] { "每年的", "周年纪念", "长期的", "年金" }, 0));
        roots.Add(wr48);

        // 49: uni
        var wr49 = new WordRoot(49, "uni", "Latin", "一", "uni", "uni 来自拉丁语「一」。uniform（制服）= uni-（统一）+ form（形式）→ 统一的形式。unique（独特的）= uni-（一）+ -que → 唯一的 → 独特的。universe（宇宙）= uni-（一）+ verse（转）→ 作为一个整体旋转的东西。");
        wr49.Examples.Add(new WordRootExample(wr49.Id, "unite", "", "uni", "te", "联合", "成为一体→联合"));
        wr49.Examples.Add(new WordRootExample(wr49.Id, "uniform", "", "uni", "form", "制服", "一种形式→制服"));
        wr49.Examples.Add(new WordRootExample(wr49.Id, "unique", "", "uni", "que", "独特的", "唯一的→独特的"));
        wr49.Examples.Add(new WordRootExample(wr49.Id, "universe", "", "uni", "verse", "宇宙", "统一世界→宇宙"));
        wr49.Examples.Add(new WordRootExample(wr49.Id, "union", "", "uni", "on", "联盟", "成一体→联盟"));
        wr49.Quizzes.Add(new WordRootQuiz(wr49.Id, "unite 的意思是什么？", new[] { "独特的", "宇宙", "制服", "联合" }, 3));
        roots.Add(wr49);

        // 50: du/duo
        var wr50 = new WordRoot(50, "du/duo", "Latin", "二", "du", "du/duo 表示「二、双」。dual（双的）、duet（二重奏）= du（二）+ -et → 两个人的表演。duplicate（复制）= du（二）+ plic（折）+ -ate → 折成两份 → 复制。");
        wr50.Examples.Add(new WordRootExample(wr50.Id, "dual", "", "du", "al", "双重的", "二的→双重的"));
        wr50.Examples.Add(new WordRootExample(wr50.Id, "duplicate", "", "du", "plicate", "复制", "使成二→复制"));
        wr50.Examples.Add(new WordRootExample(wr50.Id, "duet", "", "du", "et", "二重奏", "二人→二重奏"));
        wr50.Examples.Add(new WordRootExample(wr50.Id, "double", "", "du", "ble", "双倍的", "二的→双倍的"));
        wr50.Examples.Add(new WordRootExample(wr50.Id, "dubious", "", "dub", "ious", "怀疑的", "两种想法→怀疑的"));
        wr50.Quizzes.Add(new WordRootQuiz(wr50.Id, "dual 的意思是什么？", new[] { "双重的", "复制", "双倍的", "二重奏" }, 0));
        roots.Add(wr50);

        // 51: ab-/abs-
        var wr51 = new WordRoot(51, "ab-/abs-", "Latin", "相反，变坏，离去", "away from", "ab-/abs- 表示「离开、相反」。abnormal（异常的）= ab-（离开）+ normal（正常）→ 偏离正常 → 异常。absorb（吸收）= ab-（离开）+ sorb（吸）→ 把东西吸走。注意：元音前用 ab-，辅音前用 abs-。");
        wr51.Examples.Add(new WordRootExample(wr51.Id, "abnormal", "ab", "normal", "", "反常的", "偏离正常→反常的"));
        wr51.Examples.Add(new WordRootExample(wr51.Id, "abuse", "ab", "use", "", "滥用", "偏离正用→滥用"));
        wr51.Examples.Add(new WordRootExample(wr51.Id, "absorb", "ab", "sorb", "", "吸收", "吸掉→吸收"));
        wr51.Examples.Add(new WordRootExample(wr51.Id, "absent", "ab", "sent", "", "缺席的", "离去的→缺席的"));
        wr51.Examples.Add(new WordRootExample(wr51.Id, "abstract", "abs", "tract", "", "抽象的", "拉离→抽象的"));
        wr51.Quizzes.Add(new WordRootQuiz(wr51.Id, "absorb 的意思是什么？", new[] { "抽象的", "反常的", "吸收", "滥用" }, 2));
        roots.Add(wr51);

        // 52: ad-
        var wr52 = new WordRoot(52, "ad-", "Latin", "加强，朝向", "to, toward", "ad- 表示「朝向、加强」，是最常见的前缀之一。adapt（适应）= ad-（朝向）+ apt（合适）→ 朝着合适的方向调整 → 适应。注意：ad- 会同化，在不同辅音前变形，如 accept = ac-（朝向）+ cept（拿）。");
        wr52.Examples.Add(new WordRootExample(wr52.Id, "adapt", "ad", "apt", "", "适应", "朝向能力→适应"));
        wr52.Examples.Add(new WordRootExample(wr52.Id, "adhere", "ad", "here", "", "坚持", "粘在一起→坚持"));
        wr52.Examples.Add(new WordRootExample(wr52.Id, "adjacent", "ad", "jacent", "", "邻近的", "躺在旁边→邻近的"));
        wr52.Examples.Add(new WordRootExample(wr52.Id, "adopt", "ad", "opt", "", "采纳", "选向→采纳"));
        wr52.Examples.Add(new WordRootExample(wr52.Id, "advocate", "ad", "voc", "ate", "提倡", "朝向说→提倡"));
        wr52.Quizzes.Add(new WordRootQuiz(wr52.Id, "advocate 的意思是什么？", new[] { "适应", "采纳", "提倡", "坚持" }, 2));
        roots.Add(wr52);

        // 53: en-/em-
        var wr53 = new WordRoot(53, "en-/em-", "Latin", "进入，使", "in, into", "en-/em- 表示「使成为、进入」。enable（使能够）= en-（使）+ able（能）→ 使能够。encourage（鼓励）= en-（使）+ courage（勇气）→ 使有勇气。辅音 b/p/m 前用 em-，如 empower（授权）。");
        wr53.Examples.Add(new WordRootExample(wr53.Id, "enable", "en", "able", "", "使能够", "使能→使能够"));
        wr53.Examples.Add(new WordRootExample(wr53.Id, "encourage", "en", "courage", "", "鼓励", "使有勇气→鼓励"));
        wr53.Examples.Add(new WordRootExample(wr53.Id, "enrich", "en", "rich", "", "丰富", "使富有→丰富"));
        wr53.Examples.Add(new WordRootExample(wr53.Id, "embrace", "em", "brace", "", "拥抱", "进入怀抱→拥抱"));
        wr53.Examples.Add(new WordRootExample(wr53.Id, "employ", "em", "ploy", "", "雇用", "使用→雇用"));
        wr53.Quizzes.Add(new WordRootQuiz(wr53.Id, "enable 的意思是什么？", new[] { "鼓励", "拥抱", "使能够", "雇用" }, 2));
        roots.Add(wr53);

        // 54: circum-
        var wr54 = new WordRoot(54, "circum-", "Latin", "环绕，周围", "around", "circum- 来自拉丁语「环绕」。circumstance（环境）= circum-（周围）+ stance（站）→ 站在周围的东西 → 环境。circumference（圆周）= circum-（环绕）+ fer（带）+ -ence → 绕一圈的距离。");
        wr54.Examples.Add(new WordRootExample(wr54.Id, "circumference", "circum", "fer", "ence", "圆周", "环绕拿→圆周"));
        wr54.Examples.Add(new WordRootExample(wr54.Id, "circumstance", "circum", "st", "ance", "环境", "周围站的→环境"));
        wr54.Examples.Add(new WordRootExample(wr54.Id, "circumscribe", "circum", "scribe", "", "限制", "周围写→限制"));
        wr54.Examples.Add(new WordRootExample(wr54.Id, "circumvent", "circum", "vent", "", "规避", "绕着走→规避"));
        wr54.Quizzes.Add(new WordRootQuiz(wr54.Id, "circumscribe 的意思是什么？", new[] { "圆周", "限制", "环境", "规避" }, 1));
        roots.Add(wr54);

        // 55: con-/com-/co-
        var wr55 = new WordRoot(55, "con-/com-/co-", "Latin", "共同，一起", "together", "co-/com-/con- 表示「共同、一起」。collaborate（合作）= co-（共同）+ labor（劳动）+ -ate → 一起劳动 → 合作。这个前缀会同化：com- 用于 b/p/m 前，con- 用于其他辅音前，co- 用于元音前。");
        wr55.Examples.Add(new WordRootExample(wr55.Id, "cooperate", "co", "oper", "ate", "合作", "一起操作→合作"));
        wr55.Examples.Add(new WordRootExample(wr55.Id, "combine", "com", "bine", "", "结合", "一起二→结合"));
        wr55.Examples.Add(new WordRootExample(wr55.Id, "connect", "con", "nect", "", "连接", "一起绑→连接"));
        wr55.Examples.Add(new WordRootExample(wr55.Id, "coordinate", "co", "ordin", "ate", "协调", "一起排序→协调"));
        wr55.Examples.Add(new WordRootExample(wr55.Id, "accompany", "ac", "company", "", "陪伴", "一起→陪伴"));
        wr55.Quizzes.Add(new WordRootQuiz(wr55.Id, "connect 的意思是什么？", new[] { "协调", "合作", "连接", "结合" }, 2));
        roots.Add(wr55);

        // 56: contra-/counter-
        var wr56 = new WordRoot(56, "contra-/counter-", "Latin", "相反，对抗", "against", "contra-/counter- 表示「相反、对抗」。contradict（矛盾）= contra-（相反）+ dict（说）→ 说相反的话 → 矛盾。contrast（对比）= contra-（相反）+ st（站）→ 站在对立面 → 对比。");
        wr56.Examples.Add(new WordRootExample(wr56.Id, "contradict", "contra", "dict", "", "反驳", "相反说→反驳"));
        wr56.Examples.Add(new WordRootExample(wr56.Id, "contrary", "contr", "ary", "", "相反的", "相反的"));
        wr56.Examples.Add(new WordRootExample(wr56.Id, "counteract", "counter", "act", "", "抵消", "相反行动→抵消"));
        wr56.Examples.Add(new WordRootExample(wr56.Id, "counterpart", "counter", "part", "", "对应物", "相对部分→对应物"));
        wr56.Quizzes.Add(new WordRootQuiz(wr56.Id, "contradict 的意思是什么？", new[] { "抵消", "反驳", "对应物", "相反的" }, 1));
        roots.Add(wr56);

        // 57: fore-
        var wr57 = new WordRoot(57, "fore-", "Old English", "前，预先", "before", "fore- 表示「在前、预先」。forecast（预报）= fore-（预先）+ cast（投）→ 预先投射出来 → 预报。foresee（预见）= fore-（预先）+ see（看）→ 预先看到。和 pre- 意思相近，但 fore- 更强调「时间或空间上在前面」。");
        wr57.Examples.Add(new WordRootExample(wr57.Id, "forecast", "fore", "cast", "", "预测", "预先投→预测"));
        wr57.Examples.Add(new WordRootExample(wr57.Id, "foresee", "fore", "see", "", "预见", "预先看→预见"));
        wr57.Examples.Add(new WordRootExample(wr57.Id, "forehead", "fore", "head", "", "前额", "头的前面→前额"));
        wr57.Examples.Add(new WordRootExample(wr57.Id, "foremost", "fore", "most", "", "首要的", "最前的→首要的"));
        wr57.Quizzes.Add(new WordRootQuiz(wr57.Id, "foresee 的意思是什么？", new[] { "预测", "首要的", "前额", "预见" }, 3));
        roots.Add(wr57);

        // 58: mal-
        var wr58 = new WordRoot(58, "mal-", "Latin", "坏，恶", "bad", "mal- 表示「坏、恶」。malfunction（故障）= mal-（坏）+ function（功能）→ 功能坏了 → 故障。malicious（恶意的）= mal-（恶）+ -icious（的）→ 有恶意的。记住：mal- 的反义词是 bene-（好）。");
        wr58.Examples.Add(new WordRootExample(wr58.Id, "malfunction", "mal", "function", "", "故障", "坏的功能→故障"));
        wr58.Examples.Add(new WordRootExample(wr58.Id, "maltreat", "mal", "treat", "", "虐待", "坏对待→虐待"));
        wr58.Examples.Add(new WordRootExample(wr58.Id, "malicious", "mal", "icious", "", "恶意的", "坏意→恶意的"));
        wr58.Examples.Add(new WordRootExample(wr58.Id, "malpractice", "mal", "practice", "", "渎职", "坏实践→渎职"));
        wr58.Quizzes.Add(new WordRootQuiz(wr58.Id, "malicious 的意思是什么？", new[] { "虐待", "恶意的", "故障", "渎职" }, 1));
        roots.Add(wr58);

        // 59: mis-
        var wr59 = new WordRoot(59, "mis-", "Old English", "错，坏", "wrong, bad", "mis- 表示「错误、不当」，已在前面讲过。misfortune（不幸）= mis-（坏）+ fortune（运气）→ 坏运气 → 不幸。mislead（误导）= mis-（错）+ lead（引导）→ 引导错了 → 误导。");
        wr59.Examples.Add(new WordRootExample(wr59.Id, "mistake", "mis", "take", "", "错误", "拿错→错误"));
        wr59.Examples.Add(new WordRootExample(wr59.Id, "misunderstand", "mis", "understand", "", "误解", "错理解→误解"));
        wr59.Examples.Add(new WordRootExample(wr59.Id, "mislead", "mis", "lead", "", "误导", "错引导→误导"));
        wr59.Examples.Add(new WordRootExample(wr59.Id, "misjudge", "mis", "judge", "", "误判", "错判断→误判"));
        wr59.Quizzes.Add(new WordRootQuiz(wr59.Id, "mistake 的意思是什么？", new[] { "误解", "误导", "误判", "错误" }, 3));
        roots.Add(wr59);

        // 60: out-
        var wr60 = new WordRoot(60, "out-", "Old English", "超过，外出", "beyond, out", "out- 表示「超出、向外」，已在前面讲过。output（输出）= out-（向外）+ put（放）→ 放出来的东西 → 输出。outlook（前景）= out-（向外）+ look（看）→ 向外看到的景象 → 前景。");
        wr60.Examples.Add(new WordRootExample(wr60.Id, "outcome", "out", "come", "", "结果", "出来→结果"));
        wr60.Examples.Add(new WordRootExample(wr60.Id, "outdo", "out", "do", "", "超过", "做得更多→超过"));
        wr60.Examples.Add(new WordRootExample(wr60.Id, "outstanding", "out", "stand", "ing", "杰出的", "站出来→杰出的"));
        wr60.Examples.Add(new WordRootExample(wr60.Id, "outbreak", "out", "break", "", "爆发", "突破出来→爆发"));
        wr60.Quizzes.Add(new WordRootQuiz(wr60.Id, "outbreak 的意思是什么？", new[] { "杰出的", "爆发", "结果", "超过" }, 1));
        roots.Add(wr60);

        // 61: over-
        var wr61 = new WordRoot(61, "over-", "Old English", "过度，超过", "too much, above", "over- 表示「过度、在上」，已在前面讲过。overlap（重叠）= over-（在上）+ lap（圈）→ 一圈压在另一圈上 → 重叠。overseas（海外的）= over-（越过）+ seas（海）→ 越过大海的 → 海外的。");
        wr61.Examples.Add(new WordRootExample(wr61.Id, "overcome", "over", "come", "", "克服", "越过→克服"));
        wr61.Examples.Add(new WordRootExample(wr61.Id, "overlook", "over", "look", "", "忽略", "看过→忽略"));
        wr61.Examples.Add(new WordRootExample(wr61.Id, "overweight", "over", "weight", "", "超重", "过重→超重"));
        wr61.Examples.Add(new WordRootExample(wr61.Id, "overwhelm", "over", "whelm", "", "压倒", "压过→压倒"));
        wr61.Quizzes.Add(new WordRootQuiz(wr61.Id, "overcome 的意思是什么？", new[] { "忽略", "克服", "超重", "压倒" }, 1));
        roots.Add(wr61);

        // 62: per-
        var wr62 = new WordRoot(62, "per-", "Latin", "贯穿，自始至终", "through", "per- 表示「贯穿、彻底、完全」。perfect（完美的）= per-（完全）+ fect（做）→ 完全做好的 → 完美的。persist（坚持）= per-（彻底）+ sist（站）→ 彻底站稳 → 坚持。这是个强调「程度」的前缀。");
        wr62.Examples.Add(new WordRootExample(wr62.Id, "perfect", "per", "fect", "", "完美的", "做完→完美的"));
        wr62.Examples.Add(new WordRootExample(wr62.Id, "persist", "per", "sist", "", "坚持", "始终站→坚持"));
        wr62.Examples.Add(new WordRootExample(wr62.Id, "pervade", "per", "vade", "", "弥漫", "走遍→弥漫"));
        wr62.Examples.Add(new WordRootExample(wr62.Id, "permit", "per", "mit", "", "允许", "让通过→允许"));
        wr62.Quizzes.Add(new WordRootQuiz(wr62.Id, "permit 的意思是什么？", new[] { "弥漫", "允许", "完美的", "坚持" }, 1));
        roots.Add(wr62);

        // 63: pro-
        var wr63 = new WordRoot(63, "pro-", "Latin", "向前，在前", "forward, before", "pro- 表示「向前、支持」，已在前面讲过。promote（促进）= pro-（向前）+ mot（移动）+ -e → 向前推动 → 促进。propose（提议）= pro-（向前）+ pos（放）+ -e → 向前放出来 → 提议。");
        wr63.Examples.Add(new WordRootExample(wr63.Id, "progress", "pro", "gress", "", "进步", "向前走→进步"));
        wr63.Examples.Add(new WordRootExample(wr63.Id, "promote", "pro", "mote", "", "促进", "向前移→促进"));
        wr63.Examples.Add(new WordRootExample(wr63.Id, "project", "pro", "ject", "", "项目", "向前投→项目"));
        wr63.Examples.Add(new WordRootExample(wr63.Id, "propose", "pro", "pose", "", "提议", "向前放→提议"));
        wr63.Quizzes.Add(new WordRootQuiz(wr63.Id, "propose 的意思是什么？", new[] { "提议", "项目", "进步", "促进" }, 0));
        roots.Add(wr63);

        // 64: retro-
        var wr64 = new WordRoot(64, "retro-", "Latin", "向后，回顾", "backward", "retro- 表示「向后、回顾」。retrospect（回顾）= retro-（向后）+ spect（看）→ 向后看 → 回顾。retrograde（倒退的）= retro-（向后）+ grade（走）→ 往后走的 → 倒退的。");
        wr64.Examples.Add(new WordRootExample(wr64.Id, "retrospect", "retro", "spect", "", "回顾", "向后看→回顾"));
        wr64.Examples.Add(new WordRootExample(wr64.Id, "retrograde", "retro", "grade", "", "倒退", "向后走→倒退"));
        wr64.Examples.Add(new WordRootExample(wr64.Id, "retroactive", "retro", "act", "ive", "追溯的", "向后作用→追溯的"));
        wr64.Quizzes.Add(new WordRootQuiz(wr64.Id, "retrograde 的意思是什么？", new[] { "回顾", "倒退", "快速的", "追溯的" }, 1));
        roots.Add(wr64);

        // 65: under-
        var wr65 = new WordRoot(65, "under-", "Old English", "在下，不足", "below, beneath", "under- 表示「在下、不足」，已在前面讲过。undergo（经历）= under-（在下）+ go（走）→ 在下面走过 → 经历。undermine（削弱）= under-（在下）+ mine（挖）→ 在下面挖 → 削弱根基。");
        wr65.Examples.Add(new WordRootExample(wr65.Id, "understand", "under", "stand", "", "理解", "站在下面→理解"));
        wr65.Examples.Add(new WordRootExample(wr65.Id, "underestimate", "under", "estimate", "", "低估", "估计不足→低估"));
        wr65.Examples.Add(new WordRootExample(wr65.Id, "undergo", "under", "go", "", "经历", "从下走过→经历"));
        wr65.Examples.Add(new WordRootExample(wr65.Id, "underline", "under", "line", "", "强调", "在下划线→强调"));
        wr65.Quizzes.Add(new WordRootQuiz(wr65.Id, "understand 的意思是什么？", new[] { "低估", "强调", "经历", "理解" }, 3));
        roots.Add(wr65);

        // 66: with-
        var wr66 = new WordRoot(66, "with-", "Old English", "向后，反对", "back, against", "with- 表示「反对、向后」（古英语）。withdraw（撤退）= with-（向后）+ draw（拉）→ 向后拉 → 撤退。withstand（抵抗）= with-（反对）+ stand（站）→ 站着反对 → 抵抗。这个前缀比较少见。");
        wr66.Examples.Add(new WordRootExample(wr66.Id, "withdraw", "with", "draw", "", "撤退", "向后拉→撤退"));
        wr66.Examples.Add(new WordRootExample(wr66.Id, "withhold", "with", "hold", "", "保留", "向后拿→保留"));
        wr66.Examples.Add(new WordRootExample(wr66.Id, "withstand", "with", "stand", "", "抵抗", "反对站→抵抗"));
        wr66.Quizzes.Add(new WordRootQuiz(wr66.Id, "withdraw 的意思是什么？", new[] { "快速的", "撤退", "抵抗", "保留" }, 1));
        roots.Add(wr66);

        // 67: up-
        var wr67 = new WordRoot(67, "up-", "Old English", "向上，增强", "up, upward", "up- 表示「向上、增强」，已在前面讲过。update（更新）= up-（向上）+ date（日期）→ 更新到最新日期 → 更新。upright（直立的）= up-（向上）+ right（正）→ 向上站正 → 直立的。");
        wr67.Examples.Add(new WordRootExample(wr67.Id, "upgrade", "up", "grade", "", "升级", "向上等级→升级"));
        wr67.Examples.Add(new WordRootExample(wr67.Id, "uphold", "up", "hold", "", "支持", "向上举→支持"));
        wr67.Examples.Add(new WordRootExample(wr67.Id, "uplift", "up", "lift", "", "提升", "向上举→提升"));
        wr67.Examples.Add(new WordRootExample(wr67.Id, "upset", "up", "set", "", "打乱", "向上设置→打乱"));
        wr67.Quizzes.Add(new WordRootQuiz(wr67.Id, "upgrade 的意思是什么？", new[] { "支持", "升级", "提升", "打乱" }, 1));
        roots.Add(wr67);

        // 68: extra-
        var wr68 = new WordRoot(68, "extra-", "Latin", "超出，额外", "beyond, outside", "extra- 表示「超出、额外」。extraordinary（非凡的）= extra-（超出）+ ordinary（普通）→ 超出普通 → 非凡的。extracurricular（课外的）= extra-（额外）+ curricular（课程的）→ 课程以外的 → 课外的。");
        wr68.Examples.Add(new WordRootExample(wr68.Id, "extraordinary", "extra", "ordinary", "", "非凡的", "超出普通→非凡的"));
        wr68.Examples.Add(new WordRootExample(wr68.Id, "extracurricular", "extra", "curricular", "", "课外的", "超出课程→课外的"));
        wr68.Examples.Add(new WordRootExample(wr68.Id, "extraterrestrial", "extra", "terrestrial", "", "外星的", "超出地球→外星的"));
        wr68.Quizzes.Add(new WordRootQuiz(wr68.Id, "extracurricular 的意思是什么？", new[] { "快速的", "外星的", "课外的", "非凡的" }, 2));
        roots.Add(wr68);

        // 69: intra-/intro-
        var wr69 = new WordRoot(69, "intra-/intro-", "Latin", "内部，向内", "within, inside", "intra-/intro- 表示「内部、向内」。introduce（介绍）= intro-（向内）+ duc（引）+ -e → 引进来 → 介绍。intranet（内网）= intra-（内部）+ net（网）→ 内部网络。和 inter-（之间）区分开。");
        wr69.Examples.Add(new WordRootExample(wr69.Id, "introduce", "intro", "duce", "", "介绍", "向内引→介绍"));
        wr69.Examples.Add(new WordRootExample(wr69.Id, "introspect", "intro", "spect", "", "内省", "向内看→内省"));
        wr69.Examples.Add(new WordRootExample(wr69.Id, "intranet", "intra", "net", "", "内网", "内部网→内网"));
        wr69.Quizzes.Add(new WordRootQuiz(wr69.Id, "introduce 的意思是什么？", new[] { "介绍", "内网", "内省", "快速的" }, 0));
        roots.Add(wr69);

        // 70: ultra-
        var wr70 = new WordRoot(70, "ultra-", "Latin", "超，极", "beyond, extremely", "ultra- 表示「超、极」。ultraviolet（紫外线的）= ultra-（超）+ violet（紫色）→ 超越紫色的 → 紫外线的。ultrasound（超声波）= ultra-（超）+ sound（声音）→ 超出人耳听力范围的声音。");
        wr70.Examples.Add(new WordRootExample(wr70.Id, "ultramodern", "ultra", "modern", "", "超现代的", "超级现代→超现代的"));
        wr70.Examples.Add(new WordRootExample(wr70.Id, "ultrasound", "ultra", "sound", "", "超声波", "超级声音→超声波"));
        wr70.Examples.Add(new WordRootExample(wr70.Id, "ultraviolet", "ultra", "violet", "", "紫外线", "超紫色→紫外线"));
        wr70.Quizzes.Add(new WordRootQuiz(wr70.Id, "ultrasound 的意思是什么？", new[] { "紫外线", "超现代的", "快速的", "超声波" }, 3));
        roots.Add(wr70);

        // 71: act/ag
        var wr71 = new WordRoot(71, "act/ag", "Latin", "做，行动", "do, act", "act/ag 表示「做、行动」。action（行动）、agent（代理人）= ag（做）+ -ent（人）→ 做事的人 → 代理人。react（反应）= re-（回）+ act（做）→ 做出回应 → 反应。");
        wr71.Examples.Add(new WordRootExample(wr71.Id, "action", "", "act", "ion", "行动", "做→行动"));
        wr71.Examples.Add(new WordRootExample(wr71.Id, "active", "", "act", "ive", "积极的", "做的→积极的"));
        wr71.Examples.Add(new WordRootExample(wr71.Id, "agent", "", "ag", "ent", "代理人", "做的人→代理人"));
        wr71.Examples.Add(new WordRootExample(wr71.Id, "react", "re", "act", "", "反应", "反做→反应"));
        wr71.Examples.Add(new WordRootExample(wr71.Id, "transact", "trans", "act", "", "交易", "跨越做→交易"));
        wr71.Quizzes.Add(new WordRootQuiz(wr71.Id, "active 的意思是什么？", new[] { "代理人", "反应", "交易", "积极的" }, 3));
        roots.Add(wr71);

        // 72: ceed/cess/cede
        var wr72 = new WordRoot(72, "ceed/cess/cede", "Latin", "走，前进", "go, yield", "ceed/cess/cede 表示「走、进行」，已在前面讲过。exceed（超过）= ex-（向外）+ ceed（走）→ 走出去 → 超过。recession（衰退）= re-（回）+ cess（走）+ -ion → 往回走 → 衰退。");
        wr72.Examples.Add(new WordRootExample(wr72.Id, "proceed", "pro", "ceed", "", "前进", "向前走→前进"));
        wr72.Examples.Add(new WordRootExample(wr72.Id, "succeed", "suc", "ceed", "", "成功", "向上走→成功"));
        wr72.Examples.Add(new WordRootExample(wr72.Id, "access", "ac", "cess", "", "接近", "走向→接近"));
        wr72.Examples.Add(new WordRootExample(wr72.Id, "process", "pro", "cess", "", "过程", "向前走→过程"));
        wr72.Examples.Add(new WordRootExample(wr72.Id, "recede", "re", "cede", "", "后退", "向后走→后退"));
        wr72.Quizzes.Add(new WordRootQuiz(wr72.Id, "proceed 的意思是什么？", new[] { "前进", "后退", "接近", "成功" }, 0));
        roots.Add(wr72);

        // 73: cur/curs/cours
        var wr73 = new WordRoot(73, "cur/curs/cours", "Latin", "跑，流", "run, flow", "cur/curs/cours 表示「跑、流」。current（当前的）= cur（流）+ -ent → 正在流动的 → 当前的。occur（发生）= oc-（朝向）+ cur（跑）→ 跑过来 → 发生。course（课程）= cours（跑）+ -e → 跑的路线 → 课程。");
        wr73.Examples.Add(new WordRootExample(wr73.Id, "current", "", "cur", "ent", "当前的", "流动的→当前的"));
        wr73.Examples.Add(new WordRootExample(wr73.Id, "occur", "oc", "cur", "", "发生", "跑来→发生"));
        wr73.Examples.Add(new WordRootExample(wr73.Id, "course", "", "cours", "e", "课程", "流程→课程"));
        wr73.Examples.Add(new WordRootExample(wr73.Id, "excursion", "ex", "curs", "ion", "远足", "跑出→远足"));
        wr73.Examples.Add(new WordRootExample(wr73.Id, "recur", "re", "cur", "", "复发", "再跑→复发"));
        wr73.Quizzes.Add(new WordRootQuiz(wr73.Id, "current 的意思是什么？", new[] { "课程", "发生", "远足", "当前的" }, 3));
        roots.Add(wr73);

        // 74: fac/fact/fect/fic
        var wr74 = new WordRoot(74, "fac/fact/fect/fic", "Latin", "做，制作", "make, do", "fac/fact/fect/fic 表示「做、制作」，已在前面讲过。manufacture（制造）= manu-（手）+ fact（做）+ -ure → 用手做 → 制造。artificial（人造的）= art（艺术）+ fic（做）+ -ial → 人工做的。");
        wr74.Examples.Add(new WordRootExample(wr74.Id, "factory", "", "fact", "ory", "工厂", "制作地→工厂"));
        wr74.Examples.Add(new WordRootExample(wr74.Id, "effect", "ef", "fect", "", "效果", "做出→效果"));
        wr74.Examples.Add(new WordRootExample(wr74.Id, "sufficient", "suf", "fic", "ient", "足够的", "做够→足够的"));
        wr74.Examples.Add(new WordRootExample(wr74.Id, "manufacture", "manu", "fact", "ure", "制造", "手做→制造"));
        wr74.Examples.Add(new WordRootExample(wr74.Id, "artificial", "arti", "fic", "ial", "人造的", "艺术做→人造的"));
        wr74.Quizzes.Add(new WordRootQuiz(wr74.Id, "manufacture 的意思是什么？", new[] { "人造的", "制造", "足够的", "效果" }, 1));
        roots.Add(wr74);

        // 75: ject
        var wr75 = new WordRoot(75, "ject", "Latin", "投掷，扔", "throw", "ject 表示「投掷、扔」，已在前面讲过。inject（注射）= in-（进入）+ ject（扔）→ 扔进去 → 注射。trajectory（轨迹）= tra-（穿过）+ ject（扔）+ -ory → 扔过去的路径 → 轨迹。");
        wr75.Examples.Add(new WordRootExample(wr75.Id, "project", "pro", "ject", "", "项目", "向前投→项目"));
        wr75.Examples.Add(new WordRootExample(wr75.Id, "reject", "re", "ject", "", "拒绝", "向后扔→拒绝"));
        wr75.Examples.Add(new WordRootExample(wr75.Id, "inject", "in", "ject", "", "注射", "向内投→注射"));
        wr75.Examples.Add(new WordRootExample(wr75.Id, "object", "ob", "ject", "", "物体", "对着扔→物体"));
        wr75.Examples.Add(new WordRootExample(wr75.Id, "subject", "sub", "ject", "", "主题", "在下投→主题"));
        wr75.Quizzes.Add(new WordRootQuiz(wr75.Id, "inject 的意思是什么？", new[] { "拒绝", "物体", "主题", "注射" }, 3));
        roots.Add(wr75);

        // 76: ven/vent
        var wr76 = new WordRoot(76, "ven/vent", "Latin", "来", "come", "ven/vent 表示「来」。event（事件）= e-（向外）+ vent（来）→ 出来的事情 → 事件。prevent（预防）= pre-（提前）+ vent（来）→ 提前来到前面阻挡 → 预防。convention（大会）= con-（一起）+ ven（来）+ -tion → 一起来的聚会 → 大会。");
        wr76.Examples.Add(new WordRootExample(wr76.Id, "event", "e", "vent", "", "事件", "出来→事件"));
        wr76.Examples.Add(new WordRootExample(wr76.Id, "prevent", "pre", "vent", "", "预防", "提前来→预防"));
        wr76.Examples.Add(new WordRootExample(wr76.Id, "convention", "con", "vent", "ion", "大会", "一起来→大会"));
        wr76.Examples.Add(new WordRootExample(wr76.Id, "adventure", "ad", "vent", "ure", "冒险", "朝向来→冒险"));
        wr76.Examples.Add(new WordRootExample(wr76.Id, "intervene", "inter", "ven", "e", "干预", "之间来→干预"));
        wr76.Quizzes.Add(new WordRootQuiz(wr76.Id, "event 的意思是什么？", new[] { "冒险", "事件", "预防", "干预" }, 1));
        roots.Add(wr76);

        // 77: cap/cept/ceiv/cip
        var wr77 = new WordRoot(77, "cap/cept/ceiv/cip", "Latin", "拿，抓，接受", "take, seize", "cap/cept/ceiv/cip 表示「拿、抓、接受」。capture（捕获）= cap（抓）+ -ture → 抓住。accept（接受）= ac-（朝向）+ cept（拿）→ 朝着拿过来 → 接受。receive（收到）= re-（回）+ ceiv（拿）+ -e → 拿回来 → 收到。");
        wr77.Examples.Add(new WordRootExample(wr77.Id, "capture", "", "cap", "ture", "捕获", "抓→捕获"));
        wr77.Examples.Add(new WordRootExample(wr77.Id, "accept", "ac", "cept", "", "接受", "朝向拿→接受"));
        wr77.Examples.Add(new WordRootExample(wr77.Id, "receive", "re", "ceiv", "e", "收到", "拿回→收到"));
        wr77.Examples.Add(new WordRootExample(wr77.Id, "concept", "con", "cept", "", "概念", "一起拿→概念"));
        wr77.Examples.Add(new WordRootExample(wr77.Id, "participate", "parti", "cip", "ate", "参与", "部分拿→参与"));
        wr77.Quizzes.Add(new WordRootQuiz(wr77.Id, "receive 的意思是什么？", new[] { "概念", "参与", "捕获", "收到" }, 3));
        roots.Add(wr77);

        // 78: ten/tin/tain
        var wr78 = new WordRoot(78, "ten/tin/tain", "Latin", "拿住，保持", "hold", "ten/tin/tain 表示「拿住、保持」。contain（包含）= con-（一起）+ tain（拿住）→ 一起拿住 → 包含。maintain（维持）= main-（手）+ tain（拿）→ 用手拿住 → 维持。continuous（连续的）= con-（一起）+ tin（拿住）+ -uous → 拿在一起不放 → 连续的。");
        wr78.Examples.Add(new WordRootExample(wr78.Id, "contain", "con", "tain", "", "包含", "一起拿住→包含"));
        wr78.Examples.Add(new WordRootExample(wr78.Id, "maintain", "main", "tain", "", "维持", "手拿住→维持"));
        wr78.Examples.Add(new WordRootExample(wr78.Id, "obtain", "ob", "tain", "", "获得", "对着拿→获得"));
        wr78.Examples.Add(new WordRootExample(wr78.Id, "detain", "de", "tain", "", "拘留", "向下拿住→拘留"));
        wr78.Examples.Add(new WordRootExample(wr78.Id, "sustain", "sus", "tain", "", "支撑", "从下拿住→支撑"));
        wr78.Quizzes.Add(new WordRootQuiz(wr78.Id, "detain 的意思是什么？", new[] { "拘留", "获得", "包含", "维持" }, 0));
        roots.Add(wr78);

        // 79: pend/pens/pond
        var wr79 = new WordRoot(79, "pend/pens/pond", "Latin", "悬挂，支付", "hang, weigh, pay", "pend/pens/pond 表示「悬挂、支付」，已在前面讲过。suspend（暂停）= sus-（在下）+ pend（挂）→ 挂起来 → 暂停。compensate（补偿）= com-（一起）+ pens（支付）+ -ate → 一起支付 → 补偿。");
        wr79.Examples.Add(new WordRootExample(wr79.Id, "depend", "de", "pend", "", "依靠", "向下挂→依靠"));
        wr79.Examples.Add(new WordRootExample(wr79.Id, "suspend", "sus", "pend", "", "暂停", "从下挂→暂停"));
        wr79.Examples.Add(new WordRootExample(wr79.Id, "expense", "ex", "pens", "e", "花费", "往外支付→花费"));
        wr79.Examples.Add(new WordRootExample(wr79.Id, "independent", "in", "de", "pend+ent", "独立的", "不依靠→独立的"));
        wr79.Examples.Add(new WordRootExample(wr79.Id, "compensate", "com", "pens", "ate", "补偿", "一起支付→补偿"));
        wr79.Quizzes.Add(new WordRootQuiz(wr79.Id, "expense 的意思是什么？", new[] { "独立的", "依靠", "花费", "补偿" }, 2));
        roots.Add(wr79);

        // 80: tract
        var wr80 = new WordRoot(80, "tract", "Latin", "拉，抽", "draw, pull", "tract 表示「拉、抽」，已在前面讲过。extract（提取）= ex-（向外）+ tract（拉）→ 拉出来 → 提取。distract（分心）= dis-（分开）+ tract（拉）→ 把注意力拉开 → 分心。");
        wr80.Examples.Add(new WordRootExample(wr80.Id, "attract", "at", "tract", "", "吸引", "朝向拉→吸引"));
        wr80.Examples.Add(new WordRootExample(wr80.Id, "extract", "ex", "tract", "", "提取", "向外拉→提取"));
        wr80.Examples.Add(new WordRootExample(wr80.Id, "contract", "con", "tract", "", "合同", "一起拉→合同"));
        wr80.Examples.Add(new WordRootExample(wr80.Id, "distract", "dis", "tract", "", "分心", "分开拉→分心"));
        wr80.Examples.Add(new WordRootExample(wr80.Id, "subtract", "sub", "tract", "", "减去", "从下拉→减去"));
        wr80.Quizzes.Add(new WordRootQuiz(wr80.Id, "attract 的意思是什么？", new[] { "吸引", "分心", "合同", "提取" }, 0));
        roots.Add(wr80);

        // 81: press
        var wr81 = new WordRoot(81, "press", "Latin", "压，挤", "press", "press 表示「压」，已在前面讲过。compress（压缩）= com-（一起）+ press（压）→ 压在一起 → 压缩。suppress（压制）= sup-（在下）+ press（压）→ 压在下面 → 压制。");
        wr81.Examples.Add(new WordRootExample(wr81.Id, "pressure", "", "press", "ure", "压力", "压→压力"));
        wr81.Examples.Add(new WordRootExample(wr81.Id, "express", "ex", "press", "", "表达", "向外压→表达"));
        wr81.Examples.Add(new WordRootExample(wr81.Id, "impress", "im", "press", "", "印象", "向内压→印象"));
        wr81.Examples.Add(new WordRootExample(wr81.Id, "depress", "de", "press", "", "沮丧", "向下压→沮丧"));
        wr81.Examples.Add(new WordRootExample(wr81.Id, "compress", "com", "press", "", "压缩", "一起压→压缩"));
        wr81.Quizzes.Add(new WordRootQuiz(wr81.Id, "depress 的意思是什么？", new[] { "印象", "压力", "表达", "沮丧" }, 3));
        roots.Add(wr81);

        // 82: sist
        var wr82 = new WordRoot(82, "sist", "Latin", "站立", "stand", "sist 表示「站立」。consist（组成）= con-（一起）+ sist（站）→ 站在一起 → 组成。resist（抵抗）= re-（反）+ sist（站）→ 反着站 → 抵抗。insist（坚持）= in-（进入）+ sist（站）→ 站进去不动 → 坚持。");
        wr82.Examples.Add(new WordRootExample(wr82.Id, "assist", "as", "sist", "", "帮助", "站在旁边→帮助"));
        wr82.Examples.Add(new WordRootExample(wr82.Id, "resist", "re", "sist", "", "抵抗", "反站→抵抗"));
        wr82.Examples.Add(new WordRootExample(wr82.Id, "persist", "per", "sist", "", "坚持", "始终站→坚持"));
        wr82.Examples.Add(new WordRootExample(wr82.Id, "consist", "con", "sist", "", "由...组成", "一起站→组成"));
        wr82.Examples.Add(new WordRootExample(wr82.Id, "insist", "in", "sist", "", "坚持", "在...站→坚持"));
        wr82.Quizzes.Add(new WordRootQuiz(wr82.Id, "assist 的意思是什么？", new[] { "坚持", "抵抗", "由...组成", "帮助" }, 3));
        roots.Add(wr82);

        // 83: struct
        var wr83 = new WordRoot(83, "struct", "Latin", "建造", "build", "struct 表示「建造」，已在前面讲过。instruct（指导）= in-（进入）+ struct（建造）→ 在里面建造知识 → 指导。obstruct（阻碍）= ob-（反对）+ struct（建造）→ 建造障碍物 → 阻碍。");
        wr83.Examples.Add(new WordRootExample(wr83.Id, "structure", "", "struct", "ure", "结构", "建造→结构"));
        wr83.Examples.Add(new WordRootExample(wr83.Id, "construct", "con", "struct", "", "建设", "一起建→建设"));
        wr83.Examples.Add(new WordRootExample(wr83.Id, "instruct", "in", "struct", "", "指导", "向内建→指导"));
        wr83.Examples.Add(new WordRootExample(wr83.Id, "destroy", "de", "stroy", "", "破坏", "向下建→破坏"));
        wr83.Examples.Add(new WordRootExample(wr83.Id, "obstruct", "ob", "struct", "", "阻碍", "对着建→阻碍"));
        wr83.Quizzes.Add(new WordRootQuiz(wr83.Id, "instruct 的意思是什么？", new[] { "结构", "阻碍", "破坏", "指导" }, 3));
        roots.Add(wr83);

        // 84: form
        var wr84 = new WordRoot(84, "form", "Latin", "形状，形成", "shape, form", "form 表示「形状、形成」，已在前面讲过。inform（通知）= in-（进入）+ form（形成）→ 在脑中形成认知 → 通知。deform（变形）= de-（去除）+ form（形状）→ 去掉原来的形状 → 变形。");
        wr84.Examples.Add(new WordRootExample(wr84.Id, "format", "", "form", "at", "格式", "形状→格式"));
        wr84.Examples.Add(new WordRootExample(wr84.Id, "transform", "trans", "form", "", "转换", "跨越形成→转换"));
        wr84.Examples.Add(new WordRootExample(wr84.Id, "inform", "in", "form", "", "通知", "向内形成→通知"));
        wr84.Examples.Add(new WordRootExample(wr84.Id, "perform", "per", "form", "", "表演", "完全形成→表演"));
        wr84.Examples.Add(new WordRootExample(wr84.Id, "reform", "re", "form", "", "改革", "再形成→改革"));
        wr84.Quizzes.Add(new WordRootQuiz(wr84.Id, "transform 的意思是什么？", new[] { "通知", "格式", "表演", "转换" }, 3));
        roots.Add(wr84);

        // 85: vers/vert
        var wr85 = new WordRoot(85, "vers/vert", "Latin", "转", "turn", "vers/vert 表示「转」，已在前面讲过。reverse（颠倒）= re-（回）+ vers（转）→ 转回去 → 颠倒。divert（转移）= di-（分开）+ vert（转）→ 转到别的方向 → 转移。");
        wr85.Examples.Add(new WordRootExample(wr85.Id, "convert", "con", "vert", "", "转换", "一起转→转换"));
        wr85.Examples.Add(new WordRootExample(wr85.Id, "reverse", "re", "vers", "e", "反转", "向后转→反转"));
        wr85.Examples.Add(new WordRootExample(wr85.Id, "universe", "uni", "vers", "e", "宇宙", "统一转→宇宙"));
        wr85.Examples.Add(new WordRootExample(wr85.Id, "diverse", "di", "vers", "e", "多样的", "分开转→多样的"));
        wr85.Examples.Add(new WordRootExample(wr85.Id, "advertise", "ad", "vert", "ise", "广告", "转向→广告"));
        wr85.Quizzes.Add(new WordRootQuiz(wr85.Id, "diverse 的意思是什么？", new[] { "反转", "广告", "宇宙", "多样的" }, 3));
        roots.Add(wr85);

        // 86: plic/plex/ply
        var wr86 = new WordRoot(86, "plic/plex/ply", "Latin", "重叠，折叠", "fold", "plic/plex/ply 表示「折叠、编织」，已在前面讲过。duplicate（复制）= du-（二）+ plic（折）+ -ate → 折成两份 → 复制。complex（复杂的）= com-（一起）+ plex（折）→ 折在一起 → 复杂的。");
        wr86.Examples.Add(new WordRootExample(wr86.Id, "complex", "com", "plex", "", "复杂的", "一起叠→复杂的"));
        wr86.Examples.Add(new WordRootExample(wr86.Id, "simple", "sim", "ple", "", "简单的", "一次折→简单的"));
        wr86.Examples.Add(new WordRootExample(wr86.Id, "duplicate", "du", "plic", "ate", "复制", "双倍叠→复制"));
        wr86.Examples.Add(new WordRootExample(wr86.Id, "apply", "ap", "ply", "", "应用", "朝向叠→应用"));
        wr86.Examples.Add(new WordRootExample(wr86.Id, "reply", "re", "ply", "", "回复", "向后叠→回复"));
        wr86.Quizzes.Add(new WordRootQuiz(wr86.Id, "duplicate 的意思是什么？", new[] { "复杂的", "回复", "应用", "复制" }, 3));
        roots.Add(wr86);

        // 87: loc
        var wr87 = new WordRoot(87, "loc", "Latin", "地方", "place", "loc 表示「地方」。location（位置）= loc（地方）+ -ation → 所在的地方 → 位置。allocate（分配）= al-（朝向）+ loc（地方）+ -ate → 分配到各个地方 → 分配。local（当地的）直接来自「地方」。");
        wr87.Examples.Add(new WordRootExample(wr87.Id, "location", "", "loc", "ation", "位置", "地方→位置"));
        wr87.Examples.Add(new WordRootExample(wr87.Id, "local", "", "loc", "al", "当地的", "地方的→当地的"));
        wr87.Examples.Add(new WordRootExample(wr87.Id, "allocate", "al", "loc", "ate", "分配", "向地方→分配"));
        wr87.Examples.Add(new WordRootExample(wr87.Id, "dislocate", "dis", "loc", "ate", "脱位", "离开地方→脱位"));
        wr87.Quizzes.Add(new WordRootQuiz(wr87.Id, "location 的意思是什么？", new[] { "分配", "脱位", "位置", "当地的" }, 2));
        roots.Add(wr87);

        // 88: path
        var wr88 = new WordRoot(88, "path", "Greek", "感情，痛苦", "feeling, suffering", "path 表示「感情、痛苦」。sympathy（同情）= sym-（共同）+ path（感情）+ -y → 共同的感情 → 同情。pathetic（可怜的）= path（痛苦）+ -etic（的）→ 让人感到痛苦的 → 可怜的。");
        wr88.Examples.Add(new WordRootExample(wr88.Id, "sympathy", "sym", "path", "y", "同情", "一起感受→同情"));
        wr88.Examples.Add(new WordRootExample(wr88.Id, "empathy", "em", "path", "y", "共鸣", "进入感受→共鸣"));
        wr88.Examples.Add(new WordRootExample(wr88.Id, "apathy", "a", "path", "y", "冷漠", "无感受→冷漠"));
        wr88.Examples.Add(new WordRootExample(wr88.Id, "pathetic", "", "path", "etic", "可怜的", "痛苦的→可怜的"));
        wr88.Examples.Add(new WordRootExample(wr88.Id, "pathology", "", "path", "ology", "病理学", "疾病学→病理学"));
        wr88.Quizzes.Add(new WordRootQuiz(wr88.Id, "empathy 的意思是什么？", new[] { "可怜的", "同情", "冷漠", "共鸣" }, 3));
        roots.Add(wr88);

        // 89: log/logu/logue
        var wr89 = new WordRoot(89, "log/logu/logue", "Greek", "说，言", "word, speech", "log/logu/logue 表示「说、言、学」。dialogue（对话）= dia-（穿过）+ logue（说）→ 说来说去 → 对话。biology（生物学）= bio（生命）+ log（学）+ -y → 研究生命的学问 → 生物学。");
        wr89.Examples.Add(new WordRootExample(wr89.Id, "dialogue", "dia", "logue", "", "对话", "对着说→对话"));
        wr89.Examples.Add(new WordRootExample(wr89.Id, "monologue", "mono", "logue", "", "独白", "一个说→独白"));
        wr89.Examples.Add(new WordRootExample(wr89.Id, "logic", "", "log", "ic", "逻辑", "言说→逻辑"));
        wr89.Examples.Add(new WordRootExample(wr89.Id, "apology", "apo", "log", "y", "道歉", "说明→道歉"));
        wr89.Examples.Add(new WordRootExample(wr89.Id, "catalog", "cata", "log", "", "目录", "向下说→目录"));
        wr89.Quizzes.Add(new WordRootQuiz(wr89.Id, "dialogue 的意思是什么？", new[] { "独白", "目录", "对话", "逻辑" }, 2));
        roots.Add(wr89);

        // 90: scop
        var wr90 = new WordRoot(90, "scop", "Greek", "看，观察", "see, watch", "scop 表示「看、观察」。telescope（望远镜）= tele-（远）+ scop（看）+ -e → 看远处的工具 → 望远镜。microscope（显微镜）= micro-（微小）+ scop（看）+ -e → 看微小东西的工具 → 显微镜。");
        wr90.Examples.Add(new WordRootExample(wr90.Id, "telescope", "tele", "scop", "e", "望远镜", "远看→望远镜"));
        wr90.Examples.Add(new WordRootExample(wr90.Id, "microscope", "micro", "scop", "e", "显微镜", "微小看→显微镜"));
        wr90.Examples.Add(new WordRootExample(wr90.Id, "scope", "", "scop", "e", "范围", "看的范围→范围"));
        wr90.Examples.Add(new WordRootExample(wr90.Id, "horoscope", "horo", "scop", "e", "占星术", "看时间→占星术"));
        wr90.Quizzes.Add(new WordRootQuiz(wr90.Id, "scope 的意思是什么？", new[] { "显微镜", "范围", "望远镜", "占星术" }, 1));
        roots.Add(wr90);

        // 91: phil
        var wr91 = new WordRoot(91, "phil", "Greek", "爱", "love", "phil 表示「爱」。philosophy（哲学）= philo-（爱）+ soph（智慧）+ -y → 爱智慧 → 哲学。Philadelphia（费城）= phil（爱）+ adelph（兄弟）+ -ia → 兄弟之爱的城市 → 费城（友爱之城）。");
        wr91.Examples.Add(new WordRootExample(wr91.Id, "philosophy", "", "phil", "osophy", "哲学", "爱智慧→哲学"));
        wr91.Examples.Add(new WordRootExample(wr91.Id, "philanthropist", "", "phil", "anthropist", "慈善家", "爱人类→慈善家"));
        wr91.Examples.Add(new WordRootExample(wr91.Id, "bibliophile", "biblio", "phil", "e", "爱书者", "爱书→爱书者"));
        wr91.Examples.Add(new WordRootExample(wr91.Id, "Philadelphia", "", "phil", "adelphia", "费城", "兄弟之爱→费城"));
        wr91.Quizzes.Add(new WordRootQuiz(wr91.Id, "Philadelphia 的意思是什么？", new[] { "爱书者", "慈善家", "哲学", "费城" }, 3));
        roots.Add(wr91);

        // 92: phon
        var wr92 = new WordRoot(92, "phon", "Greek", "声音", "sound", "phon 表示「声音」。telephone（电话）= tele-（远）+ phon（声音）+ -e → 远距离传声 → 电话。symphony（交响乐）= sym-（共同）+ phon（声音）+ -y → 共同的声音 → 交响乐。");
        wr92.Examples.Add(new WordRootExample(wr92.Id, "phone", "", "phon", "e", "电话", "声音→电话"));
        wr92.Examples.Add(new WordRootExample(wr92.Id, "symphony", "sym", "phon", "y", "交响乐", "一起声音→交响乐"));
        wr92.Examples.Add(new WordRootExample(wr92.Id, "microphone", "micro", "phon", "e", "麦克风", "小声音→麦克风"));
        wr92.Examples.Add(new WordRootExample(wr92.Id, "phonetic", "", "phon", "etic", "语音的", "声音的→语音的"));
        wr92.Quizzes.Add(new WordRootQuiz(wr92.Id, "phonetic 的意思是什么？", new[] { "麦克风", "语音的", "交响乐", "电话" }, 1));
        roots.Add(wr92);

        // 93: psych
        var wr93 = new WordRoot(93, "psych", "Greek", "精神，心理", "mind, soul", "psych 表示「精神、心理」。psychology（心理学）= psych（心理）+ log（学）+ -y → 研究心理的学问。psychiatrist（精神科医生）= psych（精神）+ iatr（治疗）+ -ist（人）→ 治疗精神的人 → 精神科医生。");
        wr93.Examples.Add(new WordRootExample(wr93.Id, "psychology", "", "psych", "ology", "心理学", "心灵学→心理学"));
        wr93.Examples.Add(new WordRootExample(wr93.Id, "psychic", "", "psych", "ic", "精神的", "心灵的→精神的"));
        wr93.Examples.Add(new WordRootExample(wr93.Id, "psychiatry", "", "psych", "iatry", "精神病学", "治疗心灵→精神病学"));
        wr93.Examples.Add(new WordRootExample(wr93.Id, "psychotherapy", "", "psych", "otherapy", "心理疗法", "治疗心理→心理疗法"));
        wr93.Quizzes.Add(new WordRootQuiz(wr93.Id, "psychic 的意思是什么？", new[] { "精神的", "心理学", "精神病学", "心理疗法" }, 0));
        roots.Add(wr93);

        // 94: therm
        var wr94 = new WordRoot(94, "therm", "Greek", "热", "heat", "therm 表示「热」。thermometer（温度计）= thermo（热）+ meter（测量）→ 测量热度的工具 → 温度计。thermal（热的）= therm（热）+ -al（的）→ 关于热的。");
        wr94.Examples.Add(new WordRootExample(wr94.Id, "thermometer", "", "therm", "ometer", "温度计", "测热→温度计"));
        wr94.Examples.Add(new WordRootExample(wr94.Id, "thermal", "", "therm", "al", "热的", "热的"));
        wr94.Examples.Add(new WordRootExample(wr94.Id, "thermos", "", "therm", "os", "保温瓶", "保热→保温瓶"));
        wr94.Examples.Add(new WordRootExample(wr94.Id, "hypothermia", "hypo", "therm", "ia", "低体温", "低热→低体温"));
        wr94.Quizzes.Add(new WordRootQuiz(wr94.Id, "hypothermia 的意思是什么？", new[] { "低体温", "热的", "温度计", "保温瓶" }, 0));
        roots.Add(wr94);

        // 95: geo
        var wr95 = new WordRoot(95, "geo", "Greek", "地，地球", "earth", "geo 表示「地、地球」。geography（地理）= geo（地）+ graph（写）+ -y → 描写地球的学问 → 地理。geology（地质学）= geo（地）+ log（学）+ -y → 研究地球的学问 → 地质学。");
        wr95.Examples.Add(new WordRootExample(wr95.Id, "geography", "", "geo", "graphy", "地理", "地写→地理"));
        wr95.Examples.Add(new WordRootExample(wr95.Id, "geology", "", "geo", "logy", "地质学", "地学→地质学"));
        wr95.Examples.Add(new WordRootExample(wr95.Id, "geometry", "", "geo", "metry", "几何", "测地→几何"));
        wr95.Examples.Add(new WordRootExample(wr95.Id, "geothermal", "", "geo", "thermal", "地热的", "地热→地热的"));
        wr95.Quizzes.Add(new WordRootQuiz(wr95.Id, "geology 的意思是什么？", new[] { "地质学", "地理", "地热的", "几何" }, 0));
        roots.Add(wr95);

        // 96: hydr/hydro
        var wr96 = new WordRoot(96, "hydr/hydro", "Greek", "水", "water", "hydr/hydro 表示「水」。hydrogen（氢）= hydro（水）+ gen（产生）→ 产生水的元素 → 氢（氢燃烧生成水）。dehydrate（脱水）= de-（去除）+ hydr（水）+ -ate → 去除水分 → 脱水。");
        wr96.Examples.Add(new WordRootExample(wr96.Id, "hydraulic", "", "hydr", "aulic", "水力的", "水的→水力的"));
        wr96.Examples.Add(new WordRootExample(wr96.Id, "hydrogen", "", "hydro", "gen", "氢", "产生水→氢"));
        wr96.Examples.Add(new WordRootExample(wr96.Id, "dehydrate", "de", "hydr", "ate", "脱水", "去水→脱水"));
        wr96.Examples.Add(new WordRootExample(wr96.Id, "hydrology", "", "hydro", "logy", "水文学", "水学→水文学"));
        wr96.Quizzes.Add(new WordRootQuiz(wr96.Id, "hydrogen 的意思是什么？", new[] { "脱水", "氢", "水文学", "水力的" }, 1));
        roots.Add(wr96);

        // 97: aer/aero
        var wr97 = new WordRoot(97, "aer/aero", "Greek", "空气", "air", "aer/aero 表示「空气」。airplane（飞机）= aero（空气）+ plane（平面）→ 在空中飞的平面 → 飞机。aerobic（有氧的）= aero（空气）+ bio（生命）+ -ic → 需要空气的生命活动 → 有氧的。");
        wr97.Examples.Add(new WordRootExample(wr97.Id, "aeroplane", "", "aero", "plane", "飞机", "空中平面→飞机"));
        wr97.Examples.Add(new WordRootExample(wr97.Id, "aerobic", "", "aero", "bic", "有氧的", "空气生活→有氧的"));
        wr97.Examples.Add(new WordRootExample(wr97.Id, "aerospace", "", "aero", "space", "航空航天", "空气空间→航空航天"));
        wr97.Examples.Add(new WordRootExample(wr97.Id, "aerial", "", "aer", "ial", "空中的", "空气的→空中的"));
        wr97.Quizzes.Add(new WordRootQuiz(wr97.Id, "aerial 的意思是什么？", new[] { "有氧的", "空中的", "飞机", "航空航天" }, 1));
        roots.Add(wr97);

        // 98: astro/aster
        var wr98 = new WordRoot(98, "astro/aster", "Greek", "星星", "star", "astro/aster 表示「星星」。astronaut（宇航员）= astro（星）+ naut（航行者）→ 在星空中航行的人 → 宇航员。astronomy（天文学）= astro（星）+ nom（法则）+ -y → 研究星星运行规律的学问 → 天文学。");
        wr98.Examples.Add(new WordRootExample(wr98.Id, "astronomy", "", "astro", "nomy", "天文学", "星星规律→天文学"));
        wr98.Examples.Add(new WordRootExample(wr98.Id, "astronaut", "", "astro", "naut", "宇航员", "星星航行者→宇航员"));
        wr98.Examples.Add(new WordRootExample(wr98.Id, "asteroid", "", "aster", "oid", "小行星", "像星星→小行星"));
        wr98.Examples.Add(new WordRootExample(wr98.Id, "disaster", "dis", "aster", "", "灾难", "星星不好→灾难"));
        wr98.Quizzes.Add(new WordRootQuiz(wr98.Id, "astronomy 的意思是什么？", new[] { "天文学", "宇航员", "灾难", "小行星" }, 0));
        roots.Add(wr98);

        // 99: photo
        var wr99 = new WordRoot(99, "photo", "Greek", "光", "light", "photo 表示「光」。photograph（照片）= photo（光）+ graph（画）→ 用光画出来的图 → 照片。photosynthesis（光合作用）= photo（光）+ synthesis（合成）→ 利用光合成 → 光合作用。");
        wr99.Examples.Add(new WordRootExample(wr99.Id, "photograph", "", "photo", "graph", "照片", "光写→照片"));
        wr99.Examples.Add(new WordRootExample(wr99.Id, "photosynthesis", "", "photo", "synthesis", "光合作用", "光合成→光合作用"));
        wr99.Examples.Add(new WordRootExample(wr99.Id, "photocopy", "", "photo", "copy", "影印", "光复制→影印"));
        wr99.Examples.Add(new WordRootExample(wr99.Id, "photogenic", "", "photo", "genic", "上镜的", "产生光→上镜的"));
        wr99.Quizzes.Add(new WordRootQuiz(wr99.Id, "photocopy 的意思是什么？", new[] { "影印", "光合作用", "照片", "上镜的" }, 0));
        roots.Add(wr99);

        // 100: meter/metr
        var wr100 = new WordRoot(100, "meter/metr", "Greek", "测量", "measure", "meter/metr 表示「测量」。thermometer（温度计）= thermo（热）+ meter（测量）→ 测量温度的工具。diameter（直径）= dia-（穿过）+ meter（测量）→ 穿过圆心测量的距离 → 直径。");
        wr100.Examples.Add(new WordRootExample(wr100.Id, "thermometer", "thermo", "meter", "", "温度计", "测热→温度计"));
        wr100.Examples.Add(new WordRootExample(wr100.Id, "kilometer", "kilo", "meter", "", "千米", "千测量→千米"));
        wr100.Examples.Add(new WordRootExample(wr100.Id, "diameter", "dia", "meter", "", "直径", "穿过测量→直径"));
        wr100.Examples.Add(new WordRootExample(wr100.Id, "geometry", "geo", "metr", "y", "几何", "测地→几何"));
        wr100.Quizzes.Add(new WordRootQuiz(wr100.Id, "diameter 的意思是什么？", new[] { "几何", "直径", "千米", "温度计" }, 1));
        roots.Add(wr100);

        // 101: man/manu
        var wr101 = new WordRoot(101, "man/manu", "Latin", "手", "hand", "man/manu 表示「手」。manual（手册）= manu（手）+ -al（的）→ 手边的书 → 手册。manufacture（制造）= manu（手）+ fact（做）+ -ure → 用手做 → 制造。manicure（修指甲）= mani（手）+ cure（护理）→ 护理手 → 修指甲。");
        wr101.Examples.Add(new WordRootExample(wr101.Id, "manual", "", "manu", "al", "手册", "手的→手册"));
        wr101.Examples.Add(new WordRootExample(wr101.Id, "manufacture", "", "manu", "facture", "制造", "手做→制造"));
        wr101.Examples.Add(new WordRootExample(wr101.Id, "manuscript", "", "manu", "script", "手稿", "手写→手稿"));
        wr101.Examples.Add(new WordRootExample(wr101.Id, "manipulate", "", "mani", "pulate", "操纵", "用手→操纵"));
        wr101.Quizzes.Add(new WordRootQuiz(wr101.Id, "manufacture 的意思是什么？", new[] { "手稿", "制造", "手册", "操纵" }, 1));
        roots.Add(wr101);

        // 102: ped/pod
        var wr102 = new WordRoot(102, "ped/pod", "Latin/Greek", "脚", "foot", "ped/pod 表示「脚」。pedestrian（行人）= ped（脚）+ -estrian → 用脚走的人 → 行人。pedal（踏板）= ped（脚）+ -al → 用脚踩的东西 → 踏板。tripod（三脚架）= tri-（三）+ pod（脚）→ 三只脚的架子。注意：ped 来自拉丁语，pod 来自希腊语。");
        wr102.Examples.Add(new WordRootExample(wr102.Id, "pedestrian", "", "ped", "estrian", "行人", "用脚→行人"));
        wr102.Examples.Add(new WordRootExample(wr102.Id, "pedal", "", "ped", "al", "踏板", "脚的→踏板"));
        wr102.Examples.Add(new WordRootExample(wr102.Id, "tripod", "tri", "pod", "", "三脚架", "三脚→三脚架"));
        wr102.Examples.Add(new WordRootExample(wr102.Id, "podium", "", "pod", "ium", "讲台", "放脚的地方→讲台"));
        wr102.Quizzes.Add(new WordRootQuiz(wr102.Id, "pedestrian 的意思是什么？", new[] { "三脚架", "行人", "讲台", "踏板" }, 1));
        roots.Add(wr102);

        // 103: cap/capit
        var wr103 = new WordRoot(103, "cap/capit", "Latin", "头", "head", "cap/capit 表示「头」。captain（队长）= capit（头）+ -ain → 头头 → 队长。capital（首都）= capit（头）+ -al → 头等重要的城市 → 首都。decapitate（斩首）= de-（去除）+ capit（头）+ -ate → 去掉头 → 斩首。");
        wr103.Examples.Add(new WordRootExample(wr103.Id, "capital", "", "capit", "al", "首都", "头部→首都"));
        wr103.Examples.Add(new WordRootExample(wr103.Id, "captain", "", "capit", "ain", "船长", "头领→船长"));
        wr103.Examples.Add(new WordRootExample(wr103.Id, "decapitate", "de", "capit", "ate", "斩首", "去头→斩首"));
        wr103.Examples.Add(new WordRootExample(wr103.Id, "per capita", "per", "capita", "", "人均", "每个头→人均"));
        wr103.Quizzes.Add(new WordRootQuiz(wr103.Id, "decapitate 的意思是什么？", new[] { "船长", "首都", "斩首", "人均" }, 2));
        roots.Add(wr103);

        // 104: corp/corpor
        var wr104 = new WordRoot(104, "corp/corpor", "Latin", "身体", "body", "corp/corpor 表示「身体、团体」。corporation（公司）= corpor（团体）+ -ation → 团体组织 → 公司。corpse（尸体）= corp（身体）+ -se → 死亡的身体 → 尸体。incorporate（合并）= in-（进入）+ corpor（团体）+ -ate → 合并成一个团体。");
        wr104.Examples.Add(new WordRootExample(wr104.Id, "corporation", "", "corpor", "ation", "公司", "团体→公司"));
        wr104.Examples.Add(new WordRootExample(wr104.Id, "corpse", "", "corp", "se", "尸体", "身体→尸体"));
        wr104.Examples.Add(new WordRootExample(wr104.Id, "incorporate", "in", "corp", "orate", "合并", "进入身体→合并"));
        wr104.Examples.Add(new WordRootExample(wr104.Id, "corporal", "", "corpor", "al", "身体的", "身体的"));
        wr104.Quizzes.Add(new WordRootQuiz(wr104.Id, "corpse 的意思是什么？", new[] { "合并", "尸体", "身体的", "公司" }, 1));
        roots.Add(wr104);

        // 105: cord/cor
        var wr105 = new WordRoot(105, "cord/cor", "Latin", "心", "heart", "cord/cor 表示「心」。cordial（热情的）= cord（心）+ -ial（的）→ 发自内心的 → 热情的。record（记录）= re-（回）+ cord（心）→ 放回心里记住 → 记录。courage（勇气）= cor（心）+ -age → 心的力量 → 勇气。");
        wr105.Examples.Add(new WordRootExample(wr105.Id, "record", "re", "cord", "", "记录", "回到心里→记录"));
        wr105.Examples.Add(new WordRootExample(wr105.Id, "accord", "ac", "cord", "", "一致", "心向→一致"));
        wr105.Examples.Add(new WordRootExample(wr105.Id, "discord", "dis", "cord", "", "不和", "心分→不和"));
        wr105.Examples.Add(new WordRootExample(wr105.Id, "core", "", "cor", "e", "核心", "心→核心"));
        wr105.Quizzes.Add(new WordRootQuiz(wr105.Id, "discord 的意思是什么？", new[] { "核心", "不和", "记录", "一致" }, 1));
        roots.Add(wr105);

        // 106: dent/dont
        var wr106 = new WordRoot(106, "dent/dont", "Latin", "牙齿", "tooth", "dent/dont 表示「牙齿」。dentist（牙医）= dent（牙齿）+ -ist（人）→ 治牙齿的人 → 牙医。dental（牙科的）= dent（牙齿）+ -al（的）→ 关于牙齿的。orthodontist（正畸医生）= ortho-（正）+ dont（牙齿）+ -ist → 矫正牙齿的医生。");
        wr106.Examples.Add(new WordRootExample(wr106.Id, "dentist", "", "dent", "ist", "牙医", "牙齿专家→牙医"));
        wr106.Examples.Add(new WordRootExample(wr106.Id, "dental", "", "dent", "al", "牙齿的", "牙齿的"));
        wr106.Examples.Add(new WordRootExample(wr106.Id, "denture", "", "dent", "ure", "假牙", "牙齿→假牙"));
        wr106.Examples.Add(new WordRootExample(wr106.Id, "orthodontist", "ortho", "dont", "ist", "正畸医生", "正牙齿→正畸医生"));
        wr106.Quizzes.Add(new WordRootQuiz(wr106.Id, "orthodontist 的意思是什么？", new[] { "假牙", "牙齿的", "正畸医生", "牙医" }, 2));
        roots.Add(wr106);

        // 107: fac/face
        var wr107 = new WordRoot(107, "fac/face", "Latin", "脸，面", "face", "fac/face 表示「脸、面」。surface（表面）= sur-（在上）+ face（面）→ 在上面的一层 → 表面。interface（界面）= inter-（之间）+ face（面）→ 两者之间的接触面 → 界面。deface（损坏外观）= de-（去除）+ face（面）→ 破坏表面 → 损坏外观。");
        wr107.Examples.Add(new WordRootExample(wr107.Id, "surface", "sur", "face", "", "表面", "上面→表面"));
        wr107.Examples.Add(new WordRootExample(wr107.Id, "interface", "inter", "face", "", "界面", "之间的面→界面"));
        wr107.Examples.Add(new WordRootExample(wr107.Id, "preface", "pre", "face", "", "序言", "在前面→序言"));
        wr107.Examples.Add(new WordRootExample(wr107.Id, "facial", "", "fac", "ial", "面部的", "脸的→面部的"));
        wr107.Quizzes.Add(new WordRootQuiz(wr107.Id, "surface 的意思是什么？", new[] { "表面", "面部的", "界面", "序言" }, 0));
        roots.Add(wr107);

        // 108: greg
        var wr108 = new WordRoot(108, "greg", "Latin", "群体", "flock, gather", "greg 表示「群体、聚集」。congregation（集会）= con-（一起）+ greg（群）+ -ation → 聚成一群 → 集会。segregate（隔离）= se-（分开）+ greg（群）+ -ate → 从群体中分开 → 隔离。aggregate（总计）= ag-（朝向）+ greg（群）+ -ate → 聚到一起 → 总计。");
        wr108.Examples.Add(new WordRootExample(wr108.Id, "gregarious", "", "greg", "arious", "群居的", "群体的→群居的"));
        wr108.Examples.Add(new WordRootExample(wr108.Id, "congregate", "con", "greg", "ate", "聚集", "一起群→聚集"));
        wr108.Examples.Add(new WordRootExample(wr108.Id, "aggregate", "ag", "greg", "ate", "聚合", "向群→聚合"));
        wr108.Examples.Add(new WordRootExample(wr108.Id, "segregate", "se", "greg", "ate", "隔离", "分开群→隔离"));
        wr108.Quizzes.Add(new WordRootQuiz(wr108.Id, "gregarious 的意思是什么？", new[] { "聚集", "隔离", "群居的", "聚合" }, 2));
        roots.Add(wr108);

        // 109: habit/hibit
        var wr109 = new WordRoot(109, "habit/hibit", "Latin", "拿住，居住", "have, dwell", "habit/hibit 表示「拿住、居住」。inhabit（居住）= in-（里面）+ habit（住）→ 住在里面 → 居住。exhibit（展览）= ex-（向外）+ hibit（拿）→ 拿出来给人看 → 展览。prohibit（禁止）= pro-（在前）+ hibit（拿）→ 提前拿住阻止 → 禁止。");
        wr109.Examples.Add(new WordRootExample(wr109.Id, "habit", "", "habit", "", "习惯", "拿住→习惯"));
        wr109.Examples.Add(new WordRootExample(wr109.Id, "inhabit", "in", "habit", "", "居住", "在里面拿→居住"));
        wr109.Examples.Add(new WordRootExample(wr109.Id, "exhibit", "ex", "hibit", "", "展览", "向外拿→展览"));
        wr109.Examples.Add(new WordRootExample(wr109.Id, "prohibit", "pro", "hibit", "", "禁止", "向前拿住→禁止"));
        wr109.Quizzes.Add(new WordRootQuiz(wr109.Id, "prohibit 的意思是什么？", new[] { "展览", "居住", "习惯", "禁止" }, 3));
        roots.Add(wr109);

        // 110: leg/lect
        var wr110 = new WordRoot(110, "leg/lect", "Latin", "读，选", "read, choose", "leg/lect 表示「读、选、收集」。legend（传奇）= leg（读）+ -end → 值得阅读的故事 → 传奇。select（选择）= se-（分开）+ lect（选）→ 分开挑选 → 选择。collect（收集）= col-（一起）+ lect（收集）→ 收集到一起。");
        wr110.Examples.Add(new WordRootExample(wr110.Id, "legible", "", "leg", "ible", "可读的", "能读→可读的"));
        wr110.Examples.Add(new WordRootExample(wr110.Id, "select", "se", "lect", "", "选择", "分开选→选择"));
        wr110.Examples.Add(new WordRootExample(wr110.Id, "collect", "col", "lect", "", "收集", "一起选→收集"));
        wr110.Examples.Add(new WordRootExample(wr110.Id, "elect", "e", "lect", "", "选举", "向外选→选举"));
        wr110.Quizzes.Add(new WordRootQuiz(wr110.Id, "select 的意思是什么？", new[] { "收集", "选择", "可读的", "选举" }, 1));
        roots.Add(wr110);

        // 111: liber
        var wr111 = new WordRoot(111, "liber", "Latin", "自由", "free", "liber 表示「自由」。liberal（自由的）= liber（自由）+ -al（的）→ 自由的。liberty（自由）= liber（自由）+ -ty → 自由状态。liberate（解放）= liber（自由）+ -ate（使）→ 使自由 → 解放。");
        wr111.Examples.Add(new WordRootExample(wr111.Id, "liberty", "", "liber", "ty", "自由", "自由"));
        wr111.Examples.Add(new WordRootExample(wr111.Id, "liberal", "", "liber", "al", "自由的", "自由的"));
        wr111.Examples.Add(new WordRootExample(wr111.Id, "liberate", "", "liber", "ate", "解放", "使自由→解放"));
        wr111.Examples.Add(new WordRootExample(wr111.Id, "deliberate", "de", "liber", "ate", "深思熟虑", "完全自由→深思熟虑"));
        wr111.Quizzes.Add(new WordRootQuiz(wr111.Id, "liberal 的意思是什么？", new[] { "自由的", "解放", "深思熟虑", "自由" }, 0));
        roots.Add(wr111);

        // 112: lingu/langu
        var wr112 = new WordRoot(112, "lingu/langu", "Latin", "语言，舌头", "language, tongue", "lingu/langu 表示「语言、舌头」。language（语言）直接来自 langu。bilingual（双语的）= bi-（双）+ lingu（语言）+ -al → 会两种语言的。linguistics（语言学）= lingu（语言）+ -istics（学）→ 研究语言的学问。");
        wr112.Examples.Add(new WordRootExample(wr112.Id, "language", "", "langu", "age", "语言", "语言"));
        wr112.Examples.Add(new WordRootExample(wr112.Id, "linguist", "", "lingu", "ist", "语言学家", "语言专家→语言学家"));
        wr112.Examples.Add(new WordRootExample(wr112.Id, "bilingual", "bi", "lingu", "al", "双语的", "两种语言→双语的"));
        wr112.Examples.Add(new WordRootExample(wr112.Id, "linguistic", "", "lingu", "istic", "语言的", "语言的"));
        wr112.Quizzes.Add(new WordRootQuiz(wr112.Id, "bilingual 的意思是什么？", new[] { "双语的", "语言的", "语言学家", "语言" }, 0));
        roots.Add(wr112);

        // 113: liter
        var wr113 = new WordRoot(113, "liter", "Latin", "文字，字母", "letter", "liter 表示「文字、字母」。literature（文学）= liter（文字）+ -ature → 文字作品 → 文学。literal（字面的）= liter（字母）+ -al（的）→ 按字母理解的 → 字面的。literate（有文化的）= liter（文字）+ -ate（的）→ 认识字的 → 有文化的。");
        wr113.Examples.Add(new WordRootExample(wr113.Id, "literature", "", "liter", "ature", "文学", "文字→文学"));
        wr113.Examples.Add(new WordRootExample(wr113.Id, "literal", "", "liter", "al", "字面的", "文字的→字面的"));
        wr113.Examples.Add(new WordRootExample(wr113.Id, "literate", "", "liter", "ate", "有文化的", "懂文字→有文化的"));
        wr113.Examples.Add(new WordRootExample(wr113.Id, "illiterate", "il", "liter", "ate", "文盲的", "不懂文字→文盲的"));
        wr113.Quizzes.Add(new WordRootQuiz(wr113.Id, "illiterate 的意思是什么？", new[] { "文盲的", "有文化的", "字面的", "文学" }, 0));
        roots.Add(wr113);

        // 114: magn/maj/max
        var wr114 = new WordRoot(114, "magn/maj/max", "Latin", "大", "great, large", "magn/maj/max 表示「大」。magnificent（壮丽的）= magn（大）+ -ificent（的）→ 很大很华丽的 → 壮丽的。major（主要的）= maj（大）+ -or → 更大的 → 主要的。maximum（最大值）= max（大）+ -imum（最）→ 最大的。");
        wr114.Examples.Add(new WordRootExample(wr114.Id, "magnificent", "", "magn", "ificent", "壮丽的", "大做→壮丽的"));
        wr114.Examples.Add(new WordRootExample(wr114.Id, "major", "", "maj", "or", "主要的", "大的→主要的"));
        wr114.Examples.Add(new WordRootExample(wr114.Id, "maximum", "", "max", "imum", "最大", "最大"));
        wr114.Examples.Add(new WordRootExample(wr114.Id, "magnify", "", "magn", "ify", "放大", "使大→放大"));
        wr114.Quizzes.Add(new WordRootQuiz(wr114.Id, "maximum 的意思是什么？", new[] { "最大", "主要的", "放大", "壮丽的" }, 0));
        roots.Add(wr114);

        // 115: min/mini
        var wr115 = new WordRoot(115, "min/mini", "Latin", "小", "small", "min/mini 表示「小」。minimum（最小值）= min（小）+ -imum（最）→ 最小的。minimize（最小化）= min（小）+ -imize（使）→ 使最小化。miniature（微型的）= mini（小）+ -ature → 小型的东西 → 微型的。");
        wr115.Examples.Add(new WordRootExample(wr115.Id, "minimum", "", "min", "imum", "最小", "最小"));
        wr115.Examples.Add(new WordRootExample(wr115.Id, "minor", "", "min", "or", "较小的", "小的→较小的"));
        wr115.Examples.Add(new WordRootExample(wr115.Id, "minimize", "", "min", "imize", "最小化", "使最小→最小化"));
        wr115.Examples.Add(new WordRootExample(wr115.Id, "miniature", "", "mini", "ature", "微型", "小的→微型"));
        wr115.Quizzes.Add(new WordRootQuiz(wr115.Id, "minimum 的意思是什么？", new[] { "微型", "较小的", "最小化", "最小" }, 3));
        roots.Add(wr115);

        // 116: nov
        var wr116 = new WordRoot(116, "nov", "Latin", "新", "new", "nov 表示「新」。novel（小说）= nov（新）+ -el → 新奇的故事 → 小说。innovate（创新）= in-（进入）+ nov（新）+ -ate → 引入新东西 → 创新。renovate（翻新）= re-（再）+ nov（新）+ -ate → 重新变新 → 翻新。");
        wr116.Examples.Add(new WordRootExample(wr116.Id, "novel", "", "nov", "el", "小说", "新的→小说"));
        wr116.Examples.Add(new WordRootExample(wr116.Id, "novelty", "", "nov", "elty", "新奇", "新的→新奇"));
        wr116.Examples.Add(new WordRootExample(wr116.Id, "innovate", "in", "nov", "ate", "创新", "向内新→创新"));
        wr116.Examples.Add(new WordRootExample(wr116.Id, "renovate", "re", "nov", "ate", "翻新", "再新→翻新"));
        wr116.Quizzes.Add(new WordRootQuiz(wr116.Id, "renovate 的意思是什么？", new[] { "翻新", "创新", "新奇", "小说" }, 0));
        roots.Add(wr116);

        // 117: number/numer
        var wr117 = new WordRoot(117, "number/numer", "Latin", "数", "number", "number/numer 表示「数」。numeral（数字）= numer（数）+ -al → 数的符号 → 数字。numerous（许多的）= numer（数）+ -ous（的）→ 数量多的 → 许多的。enumerate（列举）= e-（向外）+ numer（数）+ -ate → 数出来 → 列举。");
        wr117.Examples.Add(new WordRootExample(wr117.Id, "numerous", "", "numer", "ous", "众多的", "数多→众多的"));
        wr117.Examples.Add(new WordRootExample(wr117.Id, "numeral", "", "numer", "al", "数字", "数的→数字"));
        wr117.Examples.Add(new WordRootExample(wr117.Id, "enumerate", "e", "numer", "ate", "列举", "向外数→列举"));
        wr117.Examples.Add(new WordRootExample(wr117.Id, "innumerable", "in", "numer", "able", "无数的", "不能数→无数的"));
        wr117.Quizzes.Add(new WordRootQuiz(wr117.Id, "enumerate 的意思是什么？", new[] { "列举", "数字", "众多的", "无数的" }, 0));
        roots.Add(wr117);

        // 118: ord/ordin
        var wr118 = new WordRoot(118, "ord/ordin", "Latin", "顺序", "order", "ord/ordin 表示「顺序」。order（顺序）直接来自 ord。ordinary（普通的）= ordin（顺序）+ -ary（的）→ 按常规顺序的 → 普通的。coordinate（协调）= co-（一起）+ ordin（顺序）+ -ate → 按相同顺序一起 → 协调。");
        wr118.Examples.Add(new WordRootExample(wr118.Id, "order", "", "ord", "er", "顺序", "顺序"));
        wr118.Examples.Add(new WordRootExample(wr118.Id, "ordinary", "", "ordin", "ary", "普通的", "按顺序→普通的"));
        wr118.Examples.Add(new WordRootExample(wr118.Id, "subordinate", "sub", "ordin", "ate", "下级", "在下顺序→下级"));
        wr118.Examples.Add(new WordRootExample(wr118.Id, "extraordinary", "extra", "ordin", "ary", "非凡的", "超出顺序→非凡的"));
        wr118.Quizzes.Add(new WordRootQuiz(wr118.Id, "subordinate 的意思是什么？", new[] { "非凡的", "普通的", "下级", "顺序" }, 2));
        roots.Add(wr118);

        // 119: par/peer
        var wr119 = new WordRoot(119, "par/peer", "Latin", "相等", "equal", "par/peer 表示「相等」。compare（比较）= com-（一起）+ par（相等）+ -e → 放在一起看是否相等 → 比较。peer（同辈）= peer（相等）→ 地位相等的人 → 同辈。parity（平等）= par（相等）+ -ity → 相等状态 → 平等。");
        wr119.Examples.Add(new WordRootExample(wr119.Id, "compare", "com", "par", "e", "比较", "一起等→比较"));
        wr119.Examples.Add(new WordRootExample(wr119.Id, "prepare", "pre", "par", "e", "准备", "提前等→准备"));
        wr119.Examples.Add(new WordRootExample(wr119.Id, "peer", "", "peer", "", "同伴", "相等→同伴"));
        wr119.Examples.Add(new WordRootExample(wr119.Id, "parity", "", "par", "ity", "平等", "相等→平等"));
        wr119.Quizzes.Add(new WordRootQuiz(wr119.Id, "compare 的意思是什么？", new[] { "比较", "同伴", "平等", "准备" }, 0));
        roots.Add(wr119);

        // 120: part/port
        var wr120 = new WordRoot(120, "part/port", "Latin", "部分", "part", "part/port 表示「部分」。part（部分）直接使用。participate（参与）= part（部分）+ -icip（拿）+ -ate → 拿一部分 → 参与。portion（一份）= port（部分）+ -ion → 分出的一部分 → 一份。");
        wr120.Examples.Add(new WordRootExample(wr120.Id, "partition", "", "part", "ition", "分割", "分部分→分割"));
        wr120.Examples.Add(new WordRootExample(wr120.Id, "partial", "", "part", "ial", "部分的", "部分的"));
        wr120.Examples.Add(new WordRootExample(wr120.Id, "participate", "", "part", "icipate", "参与", "成为部分→参与"));
        wr120.Examples.Add(new WordRootExample(wr120.Id, "proportion", "pro", "port", "ion", "比例", "向前部分→比例"));
        wr120.Quizzes.Add(new WordRootQuiz(wr120.Id, "participate 的意思是什么？", new[] { "比例", "部分的", "参与", "分割" }, 2));
        roots.Add(wr120);

        // 121: pass
        var wr121 = new WordRoot(121, "pass", "Latin", "通过，走", "pass", "pass 表示「通过、走」。passage（通道）= pass（通过）+ -age → 通过的地方 → 通道。passport（护照）= pass（通过）+ port（港口）→ 通过港口的文件 → 护照。surpass（超越）= sur-（超过）+ pass（走）→ 走过前面 → 超越。");
        wr121.Examples.Add(new WordRootExample(wr121.Id, "passage", "", "pass", "age", "通道", "通过→通道"));
        wr121.Examples.Add(new WordRootExample(wr121.Id, "passenger", "", "pass", "enger", "乘客", "通过的人→乘客"));
        wr121.Examples.Add(new WordRootExample(wr121.Id, "surpass", "sur", "pass", "", "超过", "在上通过→超过"));
        wr121.Examples.Add(new WordRootExample(wr121.Id, "compass", "com", "pass", "", "罗盘", "一起走→罗盘"));
        wr121.Quizzes.Add(new WordRootQuiz(wr121.Id, "passenger 的意思是什么？", new[] { "超过", "罗盘", "乘客", "通道" }, 2));
        roots.Add(wr121);

        // 122: quest/quir/quis
        var wr122 = new WordRoot(122, "quest/quir/quis", "Latin", "寻求，问", "seek, ask", "quest/quir/quis 表示「寻求、问」。question（问题）= quest（问）+ -ion → 提出来问的事情 → 问题。require（需要）= re-（回）+ quir（寻求）+ -e → 寻求得到 → 需要。acquire（获得）= ac-（朝向）+ quir（寻求）+ -e → 寻求到 → 获得。");
        wr122.Examples.Add(new WordRootExample(wr122.Id, "question", "", "quest", "ion", "问题", "问→问题"));
        wr122.Examples.Add(new WordRootExample(wr122.Id, "require", "re", "quir", "e", "需要", "反复求→需要"));
        wr122.Examples.Add(new WordRootExample(wr122.Id, "acquire", "ac", "quir", "e", "获得", "向...求→获得"));
        wr122.Examples.Add(new WordRootExample(wr122.Id, "inquire", "in", "quir", "e", "询问", "向内问→询问"));
        wr122.Quizzes.Add(new WordRootQuiz(wr122.Id, "require 的意思是什么？", new[] { "询问", "需要", "问题", "获得" }, 1));
        roots.Add(wr122);

        // 123: reg/rect
        var wr123 = new WordRoot(123, "reg/rect", "Latin", "统治，直", "rule, straight", "reg/rect 表示「统治、直」。region（地区）= reg（统治）+ -ion → 统治的区域 → 地区。correct（正确的）= cor-（完全）+ rect（直）→ 完全笔直的 → 正确的。direct（直接的）= di-（分开）+ rect（直）→ 笔直分开 → 直接的。");
        wr123.Examples.Add(new WordRootExample(wr123.Id, "regulate", "", "reg", "ulate", "调节", "规则→调节"));
        wr123.Examples.Add(new WordRootExample(wr123.Id, "correct", "cor", "rect", "", "正确", "一起直→正确"));
        wr123.Examples.Add(new WordRootExample(wr123.Id, "direct", "di", "rect", "", "直接", "直的→直接"));
        wr123.Examples.Add(new WordRootExample(wr123.Id, "rectangle", "", "rect", "angle", "矩形", "直角→矩形"));
        wr123.Quizzes.Add(new WordRootQuiz(wr123.Id, "regulate 的意思是什么？", new[] { "直接", "正确", "调节", "矩形" }, 2));
        roots.Add(wr123);

        // 124: sal/sult
        var wr124 = new WordRoot(124, "sal/sult", "Latin", "跳", "jump, leap", "sal/sult 表示「跳」。assault（攻击）= as-（朝向）+ sault（跳）→ 跳向敌人 → 攻击。result（结果）= re-（回）+ sult（跳）→ 跳回来的东西 → 结果。insult（侮辱）= in-（进入）+ sult（跳）→ 跳到脸上 → 侮辱。");
        wr124.Examples.Add(new WordRootExample(wr124.Id, "salute", "", "sal", "ute", "敬礼", "跳起→敬礼"));
        wr124.Examples.Add(new WordRootExample(wr124.Id, "assault", "as", "sault", "", "攻击", "跳向→攻击"));
        wr124.Examples.Add(new WordRootExample(wr124.Id, "result", "re", "sult", "", "结果", "跳回→结果"));
        wr124.Examples.Add(new WordRootExample(wr124.Id, "insult", "in", "sult", "", "侮辱", "跳上→侮辱"));
        wr124.Quizzes.Add(new WordRootQuiz(wr124.Id, "assault 的意思是什么？", new[] { "敬礼", "攻击", "侮辱", "结果" }, 1));
        roots.Add(wr124);

        // 125: serv
        var wr125 = new WordRoot(125, "serv", "Latin", "服务，保持", "serve, keep", "serv 表示「服务、保持」。service（服务）= serv（服务）+ -ice → 服务。preserve（保存）= pre-（提前）+ serv（保持）+ -e → 提前保持住 → 保存。deserve（值得）= de-（完全）+ serv（服务）→ 完全为之服务 → 值得。");
        wr125.Examples.Add(new WordRootExample(wr125.Id, "serve", "", "serv", "e", "服务", "服务"));
        wr125.Examples.Add(new WordRootExample(wr125.Id, "reserve", "re", "serv", "e", "保留", "保持→保留"));
        wr125.Examples.Add(new WordRootExample(wr125.Id, "preserve", "pre", "serv", "e", "保护", "提前保持→保护"));
        wr125.Examples.Add(new WordRootExample(wr125.Id, "deserve", "de", "serv", "e", "应得", "完全服务→应得"));
        wr125.Quizzes.Add(new WordRootQuiz(wr125.Id, "reserve 的意思是什么？", new[] { "服务", "应得", "保留", "保护" }, 2));
        roots.Add(wr125);

        // 126: sign
        var wr126 = new WordRoot(126, "sign", "Latin", "记号，信号", "mark, sign", "sign 表示「记号、信号」。signal（信号）= sign（记号）+ -al → 标记 → 信号。significant（重要的）= sign（记号）+ -ificant（的）→ 有标志性的 → 重要的。assign（分配）= as-（朝向）+ sign（标记）→ 做标记分配 → 分配。");
        wr126.Examples.Add(new WordRootExample(wr126.Id, "signal", "", "sign", "al", "信号", "记号→信号"));
        wr126.Examples.Add(new WordRootExample(wr126.Id, "design", "de", "sign", "", "设计", "做记号→设计"));
        wr126.Examples.Add(new WordRootExample(wr126.Id, "assign", "as", "sign", "", "分配", "向...做记号→分配"));
        wr126.Examples.Add(new WordRootExample(wr126.Id, "signature", "", "sign", "ature", "签名", "记号→签名"));
        wr126.Quizzes.Add(new WordRootQuiz(wr126.Id, "assign 的意思是什么？", new[] { "设计", "信号", "签名", "分配" }, 3));
        roots.Add(wr126);

        // 127: simil/sembl
        var wr127 = new WordRoot(127, "simil/sembl", "Latin", "相似", "like, similar", "simil/sembl 表示「相似」。similar（相似的）= simil（相似）+ -ar（的）→ 相似的。resemble（相像）= re-（回）+ sembl（相似）+ -e → 看起来相似 → 相像。assemble（集合）= as-（朝向）+ sembl（一起）+ -e → 聚到一起 → 集合。");
        wr127.Examples.Add(new WordRootExample(wr127.Id, "similar", "", "simil", "ar", "相似的", "相似的"));
        wr127.Examples.Add(new WordRootExample(wr127.Id, "resemble", "re", "sembl", "e", "像", "再相似→像"));
        wr127.Examples.Add(new WordRootExample(wr127.Id, "assemble", "as", "sembl", "e", "集合", "向...相似→集合"));
        wr127.Examples.Add(new WordRootExample(wr127.Id, "dissemble", "dis", "sembl", "e", "掩饰", "不相似→掩饰"));
        wr127.Quizzes.Add(new WordRootQuiz(wr127.Id, "similar 的意思是什么？", new[] { "掩饰", "集合", "像", "相似的" }, 3));
        roots.Add(wr127);

        // 128: sol/soli
        var wr128 = new WordRoot(128, "sol/soli", "Latin", "单独", "alone", "sol/soli 表示「单独」。sole（唯一的）= sol（单独）+ -e → 单独的 → 唯一的。solitary（孤独的）= soli（单独）+ -ary（的）→ 单独的 → 孤独的。isolate（隔离）= iso-（单独）+ -late → 使单独 → 隔离。");
        wr128.Examples.Add(new WordRootExample(wr128.Id, "solo", "", "sol", "o", "独奏", "单独→独奏"));
        wr128.Examples.Add(new WordRootExample(wr128.Id, "solitary", "", "soli", "tary", "孤独的", "单独的→孤独的"));
        wr128.Examples.Add(new WordRootExample(wr128.Id, "isolate", "i", "sol", "ate", "隔离", "使单独→隔离"));
        wr128.Examples.Add(new WordRootExample(wr128.Id, "desolate", "de", "sol", "ate", "荒凉的", "完全单独→荒凉的"));
        wr128.Quizzes.Add(new WordRootQuiz(wr128.Id, "solitary 的意思是什么？", new[] { "隔离", "荒凉的", "孤独的", "独奏" }, 2));
        roots.Add(wr128);

        // 129: spir
        var wr129 = new WordRoot(129, "spir", "Latin", "呼吸，精神", "breathe, spirit", "spir 表示「呼吸、精神」。spirit（精神）= spir（呼吸）+ -it → 呼吸 → 精神（古人认为呼吸是生命和精神的象征）。inspire（激励）= in-（进入）+ spir（呼吸）+ -e → 吹气进去 → 激励。expire（到期）= ex-（向外）+ spir（呼吸）+ -e → 呼出最后一口气 → 到期。");
        wr129.Examples.Add(new WordRootExample(wr129.Id, "spirit", "", "spir", "it", "精神", "呼吸→精神"));
        wr129.Examples.Add(new WordRootExample(wr129.Id, "inspire", "in", "spir", "e", "激励", "向内呼吸→激励"));
        wr129.Examples.Add(new WordRootExample(wr129.Id, "expire", "ex", "spir", "e", "到期", "向外呼吸→到期"));
        wr129.Examples.Add(new WordRootExample(wr129.Id, "conspire", "con", "spir", "e", "密谋", "一起呼吸→密谋"));
        wr129.Quizzes.Add(new WordRootQuiz(wr129.Id, "conspire 的意思是什么？", new[] { "密谋", "精神", "激励", "到期" }, 0));
        roots.Add(wr129);

        // 130: tang/tact/ting
        var wr130 = new WordRoot(130, "tang/tact/ting", "Latin", "触摸", "touch", "tang/tact/ting 表示「触摸」。tangible（有形的）= tang（触摸）+ -ible（能...的）→ 能触摸到的 → 有形的。contact（接触）= con-（一起）+ tact（触摸）→ 一起触摸 → 接触。intact（完整的）= in-（不）+ tact（触摸）→ 没被触碰过的 → 完整的。");
        wr130.Examples.Add(new WordRootExample(wr130.Id, "tangible", "", "tang", "ible", "有形的", "能触摸→有形的"));
        wr130.Examples.Add(new WordRootExample(wr130.Id, "contact", "con", "tact", "", "接触", "一起触→接触"));
        wr130.Examples.Add(new WordRootExample(wr130.Id, "intact", "in", "tact", "", "完整的", "不触→完整的"));
        wr130.Examples.Add(new WordRootExample(wr130.Id, "distinguish", "dis", "ting", "uish", "区分", "分开触→区分"));
        wr130.Quizzes.Add(new WordRootQuiz(wr130.Id, "intact 的意思是什么？", new[] { "接触", "有形的", "区分", "完整的" }, 3));
        roots.Add(wr130);

        // 131: -able/-ible
        var wr131 = new WordRoot(131, "-able/-ible", "Latin", "能...的，可...的", "capable of", "-able/-ible 是最常见的形容词后缀，表示「能...的、可...的」。readable = read（读）+ -able → 可读的。visible = vis（看）+ -ible → 可见的。注意：-able 用于完整单词后，-ible 多用于词根后。");
        wr131.Examples.Add(new WordRootExample(wr131.Id, "readable", "", "read", "able", "可读的", "能读→可读的"));
        wr131.Examples.Add(new WordRootExample(wr131.Id, "visible", "", "vis", "ible", "可见的", "能看→可见的"));
        wr131.Examples.Add(new WordRootExample(wr131.Id, "comfortable", "com", "fort", "able", "舒适的", "能使舒服→舒适的"));
        wr131.Examples.Add(new WordRootExample(wr131.Id, "flexible", "", "flex", "ible", "灵活的", "能弯→灵活的"));
        wr131.Quizzes.Add(new WordRootQuiz(wr131.Id, "readable 的意思是什么？", new[] { "灵活的", "可读的", "舒适的", "可见的" }, 1));
        roots.Add(wr131);

        // 132: -al/-ial
        var wr132 = new WordRoot(132, "-al/-ial", "Latin", "...的，关于...的", "of, relating to", "-al/-ial 是形容词后缀，表示「...的、关于...的」。national = nation（国家）+ -al → 国家的。social = soci（社会）+ -al → 社会的。encial = essence（本质）+ -ial → 本质的。");
        wr132.Examples.Add(new WordRootExample(wr132.Id, "natural", "", "natur", "al", "自然的", "自然的"));
        wr132.Examples.Add(new WordRootExample(wr132.Id, "social", "", "soci", "al", "社会的", "社会的"));
        wr132.Examples.Add(new WordRootExample(wr132.Id, "material", "", "mater", "ial", "物质的", "物质的"));
        wr132.Examples.Add(new WordRootExample(wr132.Id, "special", "", "spec", "ial", "特殊的", "特殊的"));
        wr132.Quizzes.Add(new WordRootQuiz(wr132.Id, "natural 的意思是什么？", new[] { "特殊的", "社会的", "物质的", "自然的" }, 3));
        roots.Add(wr132);

        // 133: -ance/-ence
        var wr133 = new WordRoot(133, "-ance/-ence", "Latin", "状态，性质", "state, quality", "-ance/-ence 是名词后缀，表示「状态、性质、行为」。importance = import（重要）+ -ance → 重要性。confidence = confid（信任）+ -ence → 信心。-ance 和 -ence 的区别主要看前面的词根。");
        wr133.Examples.Add(new WordRootExample(wr133.Id, "importance", "im", "port", "ance", "重要性", "重要状态→重要性"));
        wr133.Examples.Add(new WordRootExample(wr133.Id, "difference", "dif", "fer", "ence", "差异", "不同状态→差异"));
        wr133.Examples.Add(new WordRootExample(wr133.Id, "performance", "per", "form", "ance", "表演", "表演状态→表演"));
        wr133.Examples.Add(new WordRootExample(wr133.Id, "confidence", "con", "fid", "ence", "信心", "信任状态→信心"));
        wr133.Quizzes.Add(new WordRootQuiz(wr133.Id, "difference 的意思是什么？", new[] { "重要性", "信心", "差异", "表演" }, 2));
        roots.Add(wr133);

        // 134: -ant/-ent
        var wr134 = new WordRoot(134, "-ant/-ent", "Latin", "...的，...人", "doing, person", "-ant/-ent 既可作形容词（...的），也可作名词（...人/物）。important（重要的）、assistant（助手）用 -ant。different（不同的）、student（学生）用 -ent。");
        wr134.Examples.Add(new WordRootExample(wr134.Id, "important", "im", "port", "ant", "重要的", "重要的"));
        wr134.Examples.Add(new WordRootExample(wr134.Id, "student", "", "stud", "ent", "学生", "学习的人→学生"));
        wr134.Examples.Add(new WordRootExample(wr134.Id, "pleasant", "", "pleas", "ant", "愉快的", "愉快的"));
        wr134.Examples.Add(new WordRootExample(wr134.Id, "evident", "e", "vid", "ent", "明显的", "能看见的→明显的"));
        wr134.Quizzes.Add(new WordRootQuiz(wr134.Id, "pleasant 的意思是什么？", new[] { "明显的", "学生", "愉快的", "重要的" }, 2));
        roots.Add(wr134);

        // 135: -ate
        var wr135 = new WordRoot(135, "-ate", "Latin", "使，做", "make, do", "-ate 是动词后缀，表示「使、做」。activate = activ（活）+ -ate → 使活跃。educate = educ（引出）+ -ate → 引出知识 → 教育。也可作形容词/名词后缀。");
        wr135.Examples.Add(new WordRootExample(wr135.Id, "create", "", "cre", "ate", "创造", "使生长→创造"));
        wr135.Examples.Add(new WordRootExample(wr135.Id, "educate", "e", "duc", "ate", "教育", "引导出→教育"));
        wr135.Examples.Add(new WordRootExample(wr135.Id, "activate", "", "act", "ivate", "激活", "使行动→激活"));
        wr135.Examples.Add(new WordRootExample(wr135.Id, "separate", "se", "par", "ate", "分离", "使分开→分离"));
        wr135.Quizzes.Add(new WordRootQuiz(wr135.Id, "activate 的意思是什么？", new[] { "创造", "分离", "激活", "教育" }, 2));
        roots.Add(wr135);

        // 136: -ful
        var wr136 = new WordRoot(136, "-ful", "Old English", "充满...的", "full of", "-ful 是形容词后缀，表示「充满...的」。beautiful = beauty（美）+ -ful → 充满美的 → 美丽的。careful = care（关心）+ -ful → 充满关心的 → 小心的。注意：只有一个 l。");
        wr136.Examples.Add(new WordRootExample(wr136.Id, "beautiful", "", "beaut", "iful", "美丽的", "充满美→美丽的"));
        wr136.Examples.Add(new WordRootExample(wr136.Id, "careful", "", "care", "ful", "小心的", "充满关心→小心的"));
        wr136.Examples.Add(new WordRootExample(wr136.Id, "powerful", "", "power", "ful", "强大的", "充满力量→强大的"));
        wr136.Examples.Add(new WordRootExample(wr136.Id, "successful", "suc", "cess", "ful", "成功的", "充满成功→成功的"));
        wr136.Quizzes.Add(new WordRootQuiz(wr136.Id, "careful 的意思是什么？", new[] { "强大的", "成功的", "美丽的", "小心的" }, 3));
        roots.Add(wr136);

        // 137: -ify/-fy
        var wr137 = new WordRoot(137, "-ify/-fy", "Latin", "使，做", "make", "-ify/-fy 是动词后缀，表示「使、做」。simplify = simpl（简单）+ -ify → 使简单化。clarify = clar（清楚）+ -ify → 使清楚 → 澄清。satisfy = satis（足够）+ -fy → 使满足。");
        wr137.Examples.Add(new WordRootExample(wr137.Id, "simplify", "", "simpl", "ify", "简化", "使简单→简化"));
        wr137.Examples.Add(new WordRootExample(wr137.Id, "classify", "", "class", "ify", "分类", "使成类→分类"));
        wr137.Examples.Add(new WordRootExample(wr137.Id, "identify", "", "ident", "ify", "识别", "使相同→识别"));
        wr137.Examples.Add(new WordRootExample(wr137.Id, "satisfy", "", "satis", "fy", "满足", "使满→满足"));
        wr137.Quizzes.Add(new WordRootQuiz(wr137.Id, "identify 的意思是什么？", new[] { "简化", "满足", "识别", "分类" }, 2));
        roots.Add(wr137);

        // 138: -ing
        var wr138 = new WordRoot(138, "-ing", "Old English", "行为，过程", "action, process", "-ing 是最常用的后缀之一：①现在分词（running）②动名词（swimming）③形容词（interesting）。understanding = under（理解）+ stand（站）+ -ing → 理解。");
        wr138.Examples.Add(new WordRootExample(wr138.Id, "learning", "", "learn", "ing", "学习", "学习过程→学习"));
        wr138.Examples.Add(new WordRootExample(wr138.Id, "thinking", "", "think", "ing", "思考", "思考行为→思考"));
        wr138.Examples.Add(new WordRootExample(wr138.Id, "building", "", "build", "ing", "建筑", "建造过程→建筑"));
        wr138.Examples.Add(new WordRootExample(wr138.Id, "swimming", "", "swim", "ing", "游泳", "游泳行为→游泳"));
        wr138.Quizzes.Add(new WordRootQuiz(wr138.Id, "swimming 的意思是什么？", new[] { "建筑", "思考", "学习", "游泳" }, 3));
        roots.Add(wr138);

        // 139: -ion/-tion/-ation
        var wr139 = new WordRoot(139, "-ion/-tion/-ation", "Latin", "行为，状态，结果", "act, state, result", "-ion/-tion/-ation 是名词后缀，表示「行为、状态、结果」。action、education、creation。-tion 是最常见的形式，-ation 用于 -ate 动词后。");
        wr139.Examples.Add(new WordRootExample(wr139.Id, "action", "", "act", "ion", "行动", "行动"));
        wr139.Examples.Add(new WordRootExample(wr139.Id, "creation", "", "cre", "ation", "创造", "创造行为→创造"));
        wr139.Examples.Add(new WordRootExample(wr139.Id, "education", "e", "duc", "ation", "教育", "教育行为→教育"));
        wr139.Examples.Add(new WordRootExample(wr139.Id, "attention", "at", "tent", "ion", "注意", "注意状态→注意"));
        wr139.Quizzes.Add(new WordRootQuiz(wr139.Id, "action 的意思是什么？", new[] { "行动", "创造", "注意", "教育" }, 0));
        roots.Add(wr139);

        // 140: -ism
        var wr140 = new WordRoot(140, "-ism", "Greek/Latin", "主义，行为，状态", "doctrine, practice, state", "-ism 是名词后缀，表示「主义、行为、状态、学说」。capitalism（资本主义）、heroism（英雄主义）、tourism（旅游业）。常见于表示信仰、理论、制度的词。");
        wr140.Examples.Add(new WordRootExample(wr140.Id, "socialism", "", "social", "ism", "社会主义", "社会主义"));
        wr140.Examples.Add(new WordRootExample(wr140.Id, "realism", "", "real", "ism", "现实主义", "现实主义"));
        wr140.Examples.Add(new WordRootExample(wr140.Id, "criticism", "", "critic", "ism", "批评", "批评行为→批评"));
        wr140.Examples.Add(new WordRootExample(wr140.Id, "optimism", "", "optim", "ism", "乐观主义", "乐观主义"));
        wr140.Quizzes.Add(new WordRootQuiz(wr140.Id, "socialism 的意思是什么？", new[] { "社会主义", "现实主义", "乐观主义", "批评" }, 0));
        roots.Add(wr140);

        // 141: -ist
        var wr141 = new WordRoot(141, "-ist", "Greek/Latin", "...家，...者", "person who", "-ist 是名词后缀，表示「...的人、...家」。artist（艺术家）、scientist（科学家）、pianist（钢琴家）。通常和 -ism 配对使用。");
        wr141.Examples.Add(new WordRootExample(wr141.Id, "artist", "", "art", "ist", "艺术家", "艺术的人→艺术家"));
        wr141.Examples.Add(new WordRootExample(wr141.Id, "scientist", "", "scien", "tist", "科学家", "科学的人→科学家"));
        wr141.Examples.Add(new WordRootExample(wr141.Id, "pianist", "", "pian", "ist", "钢琴家", "钢琴的人→钢琴家"));
        wr141.Examples.Add(new WordRootExample(wr141.Id, "journalist", "", "journal", "ist", "记者", "日记的人→记者"));
        wr141.Quizzes.Add(new WordRootQuiz(wr141.Id, "journalist 的意思是什么？", new[] { "记者", "艺术家", "钢琴家", "科学家" }, 0));
        roots.Add(wr141);

        // 142: -ity/-ty
        var wr142 = new WordRoot(142, "-ity/-ty", "Latin", "性质，状态", "quality, state", "-ity/-ty 是名词后缀，表示「性质、状态」。ability = abl（能）+ -ity → 能力。quality = qual（质量）+ -ity → 质量。safety = safe（安全）+ -ty → 安全。");
        wr142.Examples.Add(new WordRootExample(wr142.Id, "ability", "", "abil", "ity", "能力", "能的性质→能力"));
        wr142.Examples.Add(new WordRootExample(wr142.Id, "reality", "", "real", "ity", "现实", "真实性质→现实"));
        wr142.Examples.Add(new WordRootExample(wr142.Id, "quality", "", "qual", "ity", "质量", "质的性质→质量"));
        wr142.Examples.Add(new WordRootExample(wr142.Id, "safety", "", "safe", "ty", "安全", "安全状态→安全"));
        wr142.Quizzes.Add(new WordRootQuiz(wr142.Id, "reality 的意思是什么？", new[] { "质量", "安全", "现实", "能力" }, 2));
        roots.Add(wr142);

        // 143: -ive
        var wr143 = new WordRoot(143, "-ive", "Latin", "...的，有...性质的", "having the nature of", "-ive 是形容词后缀，表示「...的、有...性质的」。active（积极的）、creative（创造性的）、effective（有效的）。也可作名词（detective侦探）。");
        wr143.Examples.Add(new WordRootExample(wr143.Id, "active", "", "act", "ive", "积极的", "行动的→积极的"));
        wr143.Examples.Add(new WordRootExample(wr143.Id, "creative", "", "cre", "ative", "创造性的", "创造性的"));
        wr143.Examples.Add(new WordRootExample(wr143.Id, "effective", "ef", "fect", "ive", "有效的", "有效果的→有效的"));
        wr143.Examples.Add(new WordRootExample(wr143.Id, "positive", "", "posit", "ive", "积极的", "放置的→积极的"));
        wr143.Quizzes.Add(new WordRootQuiz(wr143.Id, "positive 的意思是什么？", new[] { "快速的", "有效的", "积极的", "创造性的" }, 2));
        roots.Add(wr143);

        // 144: -ize/-ise
        var wr144 = new WordRoot(144, "-ize/-ise", "Greek/Latin", "使，做", "make, cause to be", "-less 是形容词后缀，表示「无...的、缺乏...的」。homeless = home（家）+ -less → 无家的。hopeless = hope（希望）+ -less → 无望的。和 -ful 相反。");
        wr144.Examples.Add(new WordRootExample(wr144.Id, "realize", "", "real", "ize", "实现", "使真实→实现"));
        wr144.Examples.Add(new WordRootExample(wr144.Id, "organize", "", "organ", "ize", "组织", "使有机→组织"));
        wr144.Examples.Add(new WordRootExample(wr144.Id, "modernize", "", "modern", "ize", "现代化", "使现代→现代化"));
        wr144.Examples.Add(new WordRootExample(wr144.Id, "recognize", "re", "cogn", "ize", "认出", "再知→认出"));
        wr144.Quizzes.Add(new WordRootQuiz(wr144.Id, "organize 的意思是什么？", new[] { "实现", "组织", "现代化", "认出" }, 1));
        roots.Add(wr144);

        // 145: -less
        var wr145 = new WordRoot(145, "-less", "Old English", "无，没有", "without", "-ly 是副词后缀（偶尔作形容词），表示「...地、以...方式」。quickly = quick（快）+ -ly → 快速地。friendly = friend（朋友）+ -ly → 友好的（形容词）。");
        wr145.Examples.Add(new WordRootExample(wr145.Id, "hopeless", "", "hope", "less", "绝望的", "无希望→绝望的"));
        wr145.Examples.Add(new WordRootExample(wr145.Id, "careless", "", "care", "less", "粗心的", "无关心→粗心的"));
        wr145.Examples.Add(new WordRootExample(wr145.Id, "endless", "", "end", "less", "无尽的", "无终点→无尽的"));
        wr145.Examples.Add(new WordRootExample(wr145.Id, "wireless", "", "wire", "less", "无线的", "无线→无线的"));
        wr145.Quizzes.Add(new WordRootQuiz(wr145.Id, "hopeless 的意思是什么？", new[] { "绝望的", "无尽的", "无线的", "粗心的" }, 0));
        roots.Add(wr145);

        // 146: -ly
        var wr146 = new WordRoot(146, "-ly", "Old English", "...地，...的", "in a manner", "-ment 是名词后缀，表示「行为、结果、状态」。development = develop（发展）+ -ment → 发展。government = govern（统治）+ -ment → 政府。");
        wr146.Examples.Add(new WordRootExample(wr146.Id, "quickly", "", "quick", "ly", "快速地", "快速方式→快速地"));
        wr146.Examples.Add(new WordRootExample(wr146.Id, "carefully", "", "careful", "ly", "小心地", "小心方式→小心地"));
        wr146.Examples.Add(new WordRootExample(wr146.Id, "friendly", "", "friend", "ly", "友好的", "朋友的→友好的"));
        wr146.Examples.Add(new WordRootExample(wr146.Id, "slowly", "", "slow", "ly", "慢慢地", "慢方式→慢慢地"));
        wr146.Quizzes.Add(new WordRootQuiz(wr146.Id, "slowly 的意思是什么？", new[] { "小心地", "友好的", "慢慢地", "快速地" }, 2));
        roots.Add(wr146);

        // 147: -ment
        var wr147 = new WordRoot(147, "-ment", "Latin", "行为，结果，手段", "act, result, means", "-ness 是名词后缀，表示「状态、性质」。happiness = happy（快乐）+ -ness → 快乐。kindness = kind（善良）+ -ness → 善良。最简单的名词化后缀。");
        wr147.Examples.Add(new WordRootExample(wr147.Id, "movement", "", "move", "ment", "运动", "移动行为→运动"));
        wr147.Examples.Add(new WordRootExample(wr147.Id, "development", "de", "velop", "ment", "发展", "发展过程→发展"));
        wr147.Examples.Add(new WordRootExample(wr147.Id, "agreement", "a", "gree", "ment", "协议", "同意结果→协议"));
        wr147.Examples.Add(new WordRootExample(wr147.Id, "treatment", "", "treat", "ment", "治疗", "对待行为→治疗"));
        wr147.Quizzes.Add(new WordRootQuiz(wr147.Id, "agreement 的意思是什么？", new[] { "发展", "协议", "运动", "治疗" }, 1));
        roots.Add(wr147);

        // 148: -ness
        var wr148 = new WordRoot(148, "-ness", "Old English", "状态，性质", "state, quality", "-ous/-ious 是形容词后缀，表示「充满...的、有...性质的」。famous（著名的）、dangerous（危险的）、curious（好奇的）。-ious 用于词根或以 i 结尾的词后。");
        wr148.Examples.Add(new WordRootExample(wr148.Id, "happiness", "", "happy", "ness", "幸福", "快乐状态→幸福"));
        wr148.Examples.Add(new WordRootExample(wr148.Id, "kindness", "", "kind", "ness", "善良", "善良性质→善良"));
        wr148.Examples.Add(new WordRootExample(wr148.Id, "darkness", "", "dark", "ness", "黑暗", "黑暗状态→黑暗"));
        wr148.Examples.Add(new WordRootExample(wr148.Id, "weakness", "", "weak", "ness", "虚弱", "虚弱状态→虚弱"));
        wr148.Quizzes.Add(new WordRootQuiz(wr148.Id, "darkness 的意思是什么？", new[] { "黑暗", "善良", "幸福", "虚弱" }, 0));
        roots.Add(wr148);

        // 149: -ous/-ious
        var wr149 = new WordRoot(149, "-ous/-ious", "Latin", "充满...的，有...性质的", "full of, having", "-ship 是名词后缀，表示「状态、关系、技能」。friendship（友谊）、leadership（领导力）、scholarship（奖学金）。常表示抽象的关系或地位。");
        wr149.Examples.Add(new WordRootExample(wr149.Id, "famous", "", "fam", "ous", "著名的", "充满名声→著名的"));
        wr149.Examples.Add(new WordRootExample(wr149.Id, "dangerous", "", "danger", "ous", "危险的", "充满危险→危险的"));
        wr149.Examples.Add(new WordRootExample(wr149.Id, "curious", "", "cur", "ious", "好奇的", "充满好奇→好奇的"));
        wr149.Examples.Add(new WordRootExample(wr149.Id, "previous", "pre", "vi", "ous", "先前的", "在前的→先前的"));
        wr149.Quizzes.Add(new WordRootQuiz(wr149.Id, "curious 的意思是什么？", new[] { "先前的", "著名的", "好奇的", "危险的" }, 2));
        roots.Add(wr149);

        // 150: -ure
        var wr150 = new WordRoot(150, "-ure", "Latin", "行为，结果，状态", "act, result, state", "-ure 是名词后缀，表示「行为、结果、状态」。pleasure（快乐）、pressure（压力）、failure（失败）。常用于拉丁词根后。");
        wr150.Examples.Add(new WordRootExample(wr150.Id, "failure", "", "fail", "ure", "失败", "失败行为→失败"));
        wr150.Examples.Add(new WordRootExample(wr150.Id, "pleasure", "", "pleas", "ure", "快乐", "快乐状态→快乐"));
        wr150.Examples.Add(new WordRootExample(wr150.Id, "capture", "", "capt", "ure", "捕获", "捕获行为→捕获"));
        wr150.Examples.Add(new WordRootExample(wr150.Id, "pressure", "", "press", "ure", "压力", "压的状态→压力"));
        wr150.Quizzes.Add(new WordRootQuiz(wr150.Id, "failure 的意思是什么？", new[] { "捕获", "压力", "失败", "快乐" }, 2));
        roots.Add(wr150);

        // 151: cre/cresc
        var wr151 = new WordRoot(151, "cre/cresc", "Latin", "生长", "grow", "cre/cresc 来自拉丁语「生长」。想象植物的「逐渐生长(crescendo)」过程。concrete（混凝土）= con-（一起）+ cre（生长）→ 长在一起的东西 → 凝结物。create（创造）= cre（生长）+ -ate → 让东西生长出来 → 创造。记住：increase = in-（向内）+ crease（生长）→ 向内生长 → 增加。");
        wr151.Examples.Add(new WordRootExample(wr151.Id, "create", "", "cre", "ate", "创造", "生长->创造"));
        wr151.Examples.Add(new WordRootExample(wr151.Id, "increase", "in", "cre", "ase", "增加", "向内生长->增加"));
        wr151.Examples.Add(new WordRootExample(wr151.Id, "decrease", "de", "cre", "ase", "减少", "向下生长->减少"));
        wr151.Examples.Add(new WordRootExample(wr151.Id, "concrete", "con", "cre", "te", "具体的", "一起生长->具体的"));
        wr151.Quizzes.Add(new WordRootQuiz(wr151.Id, "decrease 的意思是什么？", new[] { "减少", "具体的", "创造", "增加" }, 0));
        roots.Add(wr151);

        // 152: cred
        var wr152 = new WordRoot(152, "cred", "Latin", "相信", "believe", "cred 来自拉丁语「相信」。credit（信用）= cred（相信）+ -it → 被相信的能力 → 信用。incredible（难以置信的）= in-（不）+ cred（相信）+ -ible → 不能相信的 → 难以置信的。credentials（证书）= cred（相信）+ -entials → 让人相信的凭证。");
        wr152.Examples.Add(new WordRootExample(wr152.Id, "credit", "", "cred", "it", "信用", "相信->信用"));
        wr152.Examples.Add(new WordRootExample(wr152.Id, "incredible", "in", "cred", "ible", "难以置信", "不能信->难以置信"));
        wr152.Examples.Add(new WordRootExample(wr152.Id, "credible", "", "cred", "ible", "可信的", "能信->可信的"));
        wr152.Examples.Add(new WordRootExample(wr152.Id, "credential", "", "cred", "ential", "证书", "信用->证书"));
        wr152.Quizzes.Add(new WordRootQuiz(wr152.Id, "credential 的意思是什么？", new[] { "信用", "可信的", "证书", "难以置信" }, 2));
        roots.Add(wr152);

        // 153: don/dot
        var wr153 = new WordRoot(153, "don/dot", "Latin", "给", "give", "don/dot 表示「给予」。donate（捐赠）= don（给）+ -ate → 给出去 → 捐赠。pardon（原谅）= par-（完全）+ don（给）→ 完全给予（宽恕）→ 原谅。antidote（解毒剂）= anti-（对抗）+ dot（给）→ 给出对抗物 → 解毒剂。记住：gift 和 give 都与 don 同源。");
        wr153.Examples.Add(new WordRootExample(wr153.Id, "donate", "", "don", "ate", "捐赠", "给->捐赠"));
        wr153.Examples.Add(new WordRootExample(wr153.Id, "pardon", "par", "don", "", "原谅", "完全给->原谅"));
        wr153.Examples.Add(new WordRootExample(wr153.Id, "anecdote", "anec", "dot", "e", "轶事", "不公开给->轶事"));
        wr153.Examples.Add(new WordRootExample(wr153.Id, "antidote", "anti", "dot", "e", "解药", "反给->解药"));
        wr153.Quizzes.Add(new WordRootQuiz(wr153.Id, "antidote 的意思是什么？", new[] { "轶事", "解药", "捐赠", "原谅" }, 1));
        roots.Add(wr153);

        // 154: equ
        var wr154 = new WordRoot(154, "equ", "Latin", "相等", "equal", "equ 表示「相等」。equal（相等的）、equation（方程式）= equ（相等）+ -ation → 相等的式子。adequate（足够的）= ad-（向）+ equ（相等）+ -ate → 达到相等水平 → 足够的。equity（公平）= equ（相等）+ -ity → 相等的状态 → 公平。");
        wr154.Examples.Add(new WordRootExample(wr154.Id, "equal", "", "equ", "al", "相等", "相等"));
        wr154.Examples.Add(new WordRootExample(wr154.Id, "equation", "", "equ", "ation", "方程", "相等->方程"));
        wr154.Examples.Add(new WordRootExample(wr154.Id, "adequate", "ad", "equ", "ate", "足够的", "向...等->足够的"));
        wr154.Examples.Add(new WordRootExample(wr154.Id, "equity", "", "equ", "ity", "公平", "相等->公平"));
        wr154.Quizzes.Add(new WordRootQuiz(wr154.Id, "adequate 的意思是什么？", new[] { "足够的", "方程", "公平", "相等" }, 0));
        roots.Add(wr154);

        // 155: flu/flux
        var wr155 = new WordRoot(155, "flu/flux", "Latin", "流", "flow", "flu/flux 表示「流动」。fluid（流体）、fluent（流利的）= flu（流）+ -ent → 流动的 → 流利的。influence（影响）= in-（进入）+ flu（流）+ -ence → 流进来 → 影响。influx（涌入）= in-（进入）+ flux（流）→ 流入 → 涌入。记住：flush（冲洗）也来自这个词根。");
        wr155.Examples.Add(new WordRootExample(wr155.Id, "fluid", "", "flu", "id", "流体", "流->流体"));
        wr155.Examples.Add(new WordRootExample(wr155.Id, "influence", "in", "flu", "ence", "影响", "流入->影响"));
        wr155.Examples.Add(new WordRootExample(wr155.Id, "fluent", "", "flu", "ent", "流利的", "流动->流利的"));
        wr155.Examples.Add(new WordRootExample(wr155.Id, "affluent", "af", "flu", "ent", "富裕的", "流向->富裕的"));
        wr155.Quizzes.Add(new WordRootQuiz(wr155.Id, "influence 的意思是什么？", new[] { "富裕的", "流体", "流利的", "影响" }, 3));
        roots.Add(wr155);

        // 156: found/fund
        var wr156 = new WordRoot(156, "found/fund", "Latin", "基础", "base", "found/fund 表示「基础、底部」。foundation（基础）= found（基）+ -ation → 打地基。profound（深刻的）= pro-（向前）+ found（底）→ 深到底部的 → 深刻的。fundamental（基本的）= fund（基）+ -amental → 基础的。");
        wr156.Examples.Add(new WordRootExample(wr156.Id, "found", "", "found", "", "建立", "基础->建立"));
        wr156.Examples.Add(new WordRootExample(wr156.Id, "foundation", "", "found", "ation", "基础", "基础"));
        wr156.Examples.Add(new WordRootExample(wr156.Id, "fundamental", "", "fund", "amental", "基本的", "基础的->基本的"));
        wr156.Examples.Add(new WordRootExample(wr156.Id, "profound", "pro", "found", "", "深刻的", "向前基础->深刻的"));
        wr156.Quizzes.Add(new WordRootQuiz(wr156.Id, "found 的意思是什么？", new[] { "基础", "深刻的", "建立", "基本的" }, 2));
        roots.Add(wr156);

        // 157: grat
        var wr157 = new WordRoot(157, "grat", "Latin", "感激", "thanks", "grat 表示「感激、令人愉快」。grateful（感激的）、gratitude（感恩）= grat（感激）+ -itude。congratulate（祝贺）= con-（一起）+ grat（高兴）+ -ulate → 一起高兴 → 祝贺。ingratiate（讨好）= in-（使）+ grat（令人愉快）+ -iate → 使人高兴 → 讨好。");
        wr157.Examples.Add(new WordRootExample(wr157.Id, "grateful", "", "grat", "eful", "感激的", "感激的"));
        wr157.Examples.Add(new WordRootExample(wr157.Id, "gratitude", "", "grat", "itude", "感激", "感激"));
        wr157.Examples.Add(new WordRootExample(wr157.Id, "congratulate", "con", "grat", "ulate", "祝贺", "一起感激->祝贺"));
        wr157.Examples.Add(new WordRootExample(wr157.Id, "ingratiate", "in", "grat", "iate", "讨好", "向内感激->讨好"));
        wr157.Quizzes.Add(new WordRootQuiz(wr157.Id, "gratitude 的意思是什么？", new[] { "感激", "感激的", "祝贺", "讨好" }, 0));
        roots.Add(wr157);

        // 158: her/hes
        var wr158 = new WordRoot(158, "her/hes", "Latin", "粘", "stick", "her/hes 表示「粘附、坚持」。adhere（坚持）= ad-（向）+ here（粘）→ 粘着 → 坚持。coherent（连贯的）= co-（一起）+ her（粘）+ -ent → 粘在一起的 → 连贯的。hesitate（犹豫）= hes（粘）+ -itate → 粘住不动 → 犹豫。");
        wr158.Examples.Add(new WordRootExample(wr158.Id, "adhere", "ad", "here", "", "粘附", "向...粘->粘附"));
        wr158.Examples.Add(new WordRootExample(wr158.Id, "cohere", "co", "here", "", "连贯", "一起粘->连贯"));
        wr158.Examples.Add(new WordRootExample(wr158.Id, "inherent", "in", "her", "ent", "固有的", "在里粘->固有的"));
        wr158.Examples.Add(new WordRootExample(wr158.Id, "hesitate", "", "hes", "itate", "犹豫", "粘住->犹豫"));
        wr158.Quizzes.Add(new WordRootQuiz(wr158.Id, "hesitate 的意思是什么？", new[] { "犹豫", "粘附", "固有的", "连贯" }, 0));
        roots.Add(wr158);

        // 159: jud/judic
        var wr159 = new WordRoot(159, "jud/judic", "Latin", "判断", "judge", "jud/judic 表示「判断」。judge（法官）、prejudice（偏见）= pre-（提前）+ judice（判断）→ 提前判断 → 偏见。judicial（司法的）= judic（判断）+ -ial → 判断的 → 司法的。记住：justice（正义）也来自这个词根家族。");
        wr159.Examples.Add(new WordRootExample(wr159.Id, "judge", "", "jud", "ge", "判断", "判断"));
        wr159.Examples.Add(new WordRootExample(wr159.Id, "prejudice", "pre", "jud", "ice", "偏见", "预先判断->偏见"));
        wr159.Examples.Add(new WordRootExample(wr159.Id, "judicial", "", "judic", "ial", "司法的", "判断的->司法的"));
        wr159.Examples.Add(new WordRootExample(wr159.Id, "adjudicate", "ad", "judic", "ate", "裁决", "向...判断->裁决"));
        wr159.Quizzes.Add(new WordRootQuiz(wr159.Id, "judge 的意思是什么？", new[] { "司法的", "判断", "裁决", "偏见" }, 1));
        roots.Add(wr159);

        // 160: jur/jus
        var wr160 = new WordRoot(160, "jur/jus", "Latin", "法律,发誓", "law, swear", "jur/jus 表示「法律、发誓」。jury（陪审团）、justice（正义）= jus（法律）+ -tice。injure（伤害）= in-（违反）+ jur（法律）→ 违反正义 → 伤害。perjury（伪证）= per-（彻底）+ jur（发誓）+ -y → 违背誓言 → 伪证。");
        wr160.Examples.Add(new WordRootExample(wr160.Id, "jury", "", "jur", "y", "陪审团", "发誓->陪审团"));
        wr160.Examples.Add(new WordRootExample(wr160.Id, "justice", "", "jus", "tice", "正义", "法律->正义"));
        wr160.Examples.Add(new WordRootExample(wr160.Id, "injury", "in", "jur", "y", "伤害", "不法->伤害"));
        wr160.Examples.Add(new WordRootExample(wr160.Id, "adjust", "ad", "just", "", "调整", "向...正->调整"));
        wr160.Quizzes.Add(new WordRootQuiz(wr160.Id, "jury 的意思是什么？", new[] { "陪审团", "正义", "伤害", "调整" }, 0));
        roots.Add(wr160);

        // 161: later
        var wr161 = new WordRoot(161, "later", "Latin", "边", "side", "later 表示「边、侧面」。lateral（侧面的）、collateral（抵押品）= col-（一起）+ later（边）+ -al → 在旁边的东西 → 抵押品。unilateral（单边的）= uni-（一）+ later（边）+ -al → 一边的。");
        wr161.Examples.Add(new WordRootExample(wr161.Id, "lateral", "", "later", "al", "侧面的", "边的->侧面的"));
        wr161.Examples.Add(new WordRootExample(wr161.Id, "bilateral", "bi", "later", "al", "双边的", "两边->双边的"));
        wr161.Examples.Add(new WordRootExample(wr161.Id, "unilateral", "uni", "later", "al", "单边的", "一边->单边的"));
        wr161.Examples.Add(new WordRootExample(wr161.Id, "collateral", "col", "later", "al", "抵押品", "一起边->抵押品"));
        wr161.Quizzes.Add(new WordRootQuiz(wr161.Id, "lateral 的意思是什么？", new[] { "抵押品", "单边的", "侧面的", "双边的" }, 2));
        roots.Add(wr161);

        // 162: lev
        var wr162 = new WordRoot(162, "lev", "Latin", "举起", "raise", "lev 表示「举起、轻」。elevator（电梯）= e-（向外）+ lev（举）+ -ator → 举起来的东西。lever（杠杆）= lev（举）+ -er → 举东西的工具。relevant（相关的）= re-（再）+ lev（举）+ -ant → 再次提起的 → 相关的。relieve（减轻）= re-（再）+ liev（举）→ 举起负担 → 减轻。");
        wr162.Examples.Add(new WordRootExample(wr162.Id, "elevate", "e", "lev", "ate", "提升", "向外举->提升"));
        wr162.Examples.Add(new WordRootExample(wr162.Id, "elevator", "e", "lev", "ator", "电梯", "举起器->电梯"));
        wr162.Examples.Add(new WordRootExample(wr162.Id, "relieve", "re", "liev", "e", "减轻", "再举->减轻"));
        wr162.Examples.Add(new WordRootExample(wr162.Id, "lever", "", "lev", "er", "杠杆", "举起->杠杆"));
        wr162.Quizzes.Add(new WordRootQuiz(wr162.Id, "lever 的意思是什么？", new[] { "提升", "杠杆", "电梯", "减轻" }, 1));
        roots.Add(wr162);

        // 163: liqu
        var wr163 = new WordRoot(163, "liqu", "Latin", "液体", "liquid", "liqu 表示「液体」。liquid（液体）、liquidate（清算）= liqu（液体）+ -idate → 变成液体 → 清算（资产变现）。liquor（烈酒）直接来自 liqu。记住：liqu 暗示「流动性」，所以 liquid assets（流动资产）很形象。");
        wr163.Examples.Add(new WordRootExample(wr163.Id, "liquid", "", "liqu", "id", "液体", "液体"));
        wr163.Examples.Add(new WordRootExample(wr163.Id, "liquidate", "", "liqu", "idate", "清算", "变液体->清算"));
        wr163.Examples.Add(new WordRootExample(wr163.Id, "liquor", "", "liqu", "or", "酒", "液体->酒"));
        wr163.Quizzes.Add(new WordRootQuiz(wr163.Id, "liquidate 的意思是什么？", new[] { "清算", "液体", "酒", "快速的" }, 0));
        roots.Add(wr163);

        // 164: mark
        var wr164 = new WordRoot(164, "mark", "Germanic", "标记", "mark", "mark 来自日耳曼语「标记」。market（市场）= mark（标记）+ -et → 有标记的地方 → 市场（古代市场用标记划分摊位）。landmark（地标）= land（土地）+ mark（标记）→ 土地上的标记。trademark（商标）= trade（贸易）+ mark（标记）。");
        wr164.Examples.Add(new WordRootExample(wr164.Id, "mark", "", "mark", "", "标记", "标记"));
        wr164.Examples.Add(new WordRootExample(wr164.Id, "remark", "re", "mark", "", "评论", "再标记->评论"));
        wr164.Examples.Add(new WordRootExample(wr164.Id, "landmark", "land", "mark", "", "地标", "土地标记->地标"));
        wr164.Examples.Add(new WordRootExample(wr164.Id, "bookmark", "book", "mark", "", "书签", "书标记->书签"));
        wr164.Quizzes.Add(new WordRootQuiz(wr164.Id, "bookmark 的意思是什么？", new[] { "标记", "地标", "评论", "书签" }, 3));
        roots.Add(wr164);

        // 165: medi
        var wr165 = new WordRoot(165, "medi", "Latin", "中间", "middle", "medi 表示「中间」。medium（中等的）、mediate（调解）= medi（中间）+ -ate → 站在中间 → 调解。medieval（中世纪的）= medi（中间）+ ev（时代）+ -al → 中间时代的。immediate（立即的）= im-（不）+ medi（中间）+ -ate → 没有中间环节的 → 立即的。");
        wr165.Examples.Add(new WordRootExample(wr165.Id, "medium", "", "medi", "um", "中等", "中间->中等"));
        wr165.Examples.Add(new WordRootExample(wr165.Id, "mediate", "", "medi", "ate", "调解", "居中->调解"));
        wr165.Examples.Add(new WordRootExample(wr165.Id, "immediate", "im", "medi", "ate", "立即的", "不居中->立即的"));
        wr165.Examples.Add(new WordRootExample(wr165.Id, "medieval", "", "medi", "eval", "中世纪", "中间时代->中世纪"));
        wr165.Quizzes.Add(new WordRootQuiz(wr165.Id, "immediate 的意思是什么？", new[] { "立即的", "中世纪", "中等", "调解" }, 0));
        roots.Add(wr165);

        // 166: migr
        var wr166 = new WordRoot(166, "migr", "Latin", "迁移", "move", "migr 表示「迁移」。migrate（迁徙）、immigrant（移民）= im-（进入）+ migr（迁移）+ -ant → 迁移进来的人。emigrate（移出）= e-（向外）+ migr（迁移）+ -ate → 迁移出去。记住：候鸟叫 migratory birds。");
        wr166.Examples.Add(new WordRootExample(wr166.Id, "migrate", "", "migr", "ate", "迁移", "迁移"));
        wr166.Examples.Add(new WordRootExample(wr166.Id, "immigrate", "im", "migr", "ate", "移入", "向内迁->移入"));
        wr166.Examples.Add(new WordRootExample(wr166.Id, "emigrate", "e", "migr", "ate", "移出", "向外迁->移出"));
        wr166.Examples.Add(new WordRootExample(wr166.Id, "immigrant", "im", "migr", "ant", "移民", "迁入者->移民"));
        wr166.Quizzes.Add(new WordRootQuiz(wr166.Id, "immigrate 的意思是什么？", new[] { "移民", "移入", "移出", "迁移" }, 1));
        roots.Add(wr166);

        // 167: mod
        var wr167 = new WordRoot(167, "mod", "Latin", "方式,度量", "manner, measure", "mod 表示「方式、度量、适度」。mode（模式）、moderate（适度的）= mod（度量）+ -erate → 有度量的。modify（修改）= mod（方式）+ -ify → 改变方式。accommodate（容纳）= ac-（向）+ com-（一起）+ mod（适应）+ -ate → 使适应 → 容纳。");
        wr167.Examples.Add(new WordRootExample(wr167.Id, "mode", "", "mod", "e", "模式", "方式->模式"));
        wr167.Examples.Add(new WordRootExample(wr167.Id, "model", "", "mod", "el", "模型", "度量->模型"));
        wr167.Examples.Add(new WordRootExample(wr167.Id, "moderate", "", "mod", "erate", "适度的", "有度量->适度的"));
        wr167.Examples.Add(new WordRootExample(wr167.Id, "modify", "", "mod", "ify", "修改", "改变方式->修改"));
        wr167.Quizzes.Add(new WordRootQuiz(wr167.Id, "modify 的意思是什么？", new[] { "模型", "适度的", "模式", "修改" }, 3));
        roots.Add(wr167);

        // 168: mot/mob/mov
        var wr168 = new WordRoot(168, "mot/mob/mov", "Latin", "移动", "move", "mot/mob/mov 表示「移动」。motion（运动）、mobile（移动的）= mob（动）+ -ile → 能动的。automobile（汽车）= auto-（自己）+ mobile（移动）→ 自己会动的。promote（促进）= pro-（向前）+ mot（动）→ 向前推动 → 促进。");
        wr168.Examples.Add(new WordRootExample(wr168.Id, "motion", "", "mot", "ion", "运动", "移动->运动"));
        wr168.Examples.Add(new WordRootExample(wr168.Id, "mobile", "", "mob", "ile", "移动的", "能动->移动的"));
        wr168.Examples.Add(new WordRootExample(wr168.Id, "remove", "re", "mov", "e", "移除", "再移动->移除"));
        wr168.Examples.Add(new WordRootExample(wr168.Id, "promote", "pro", "mot", "e", "促进", "向前移->促进"));
        wr168.Quizzes.Add(new WordRootQuiz(wr168.Id, "remove 的意思是什么？", new[] { "促进", "移除", "运动", "移动的" }, 1));
        roots.Add(wr168);

        // 169: nat/nasc
        var wr169 = new WordRoot(169, "nat/nasc", "Latin", "出生", "born", "nat/nasc 表示「出生」。nature（自然）、native（本地的）= nat（出生）+ -ive → 出生的地方。renaissance（文艺复兴）= re-（再）+ naiss（生）+ -ance → 再生 → 文艺复兴。nascent（新生的）= nasc（生）+ -ent → 正在出生的。");
        wr169.Examples.Add(new WordRootExample(wr169.Id, "nature", "", "nat", "ure", "自然", "出生->自然"));
        wr169.Examples.Add(new WordRootExample(wr169.Id, "native", "", "nat", "ive", "本地的", "出生的->本地的"));
        wr169.Examples.Add(new WordRootExample(wr169.Id, "nascent", "", "nasc", "ent", "新生的", "正出生->新生的"));
        wr169.Examples.Add(new WordRootExample(wr169.Id, "innate", "in", "nat", "e", "天生的", "在内出生->天生的"));
        wr169.Quizzes.Add(new WordRootQuiz(wr169.Id, "nature 的意思是什么？", new[] { "新生的", "本地的", "天生的", "自然" }, 3));
        roots.Add(wr169);

        // 170: neg
        var wr170 = new WordRoot(170, "neg", "Latin", "否定", "deny", "neg 表示「否定」。negative（消极的）、neglect（忽视）= neg（否定）+ lect（选择）→ 不选择 → 忽视。negate（否定）= neg（否定）+ -ate。negotiate（谈判）= neg（否定）+ oti（闲暇）+ -ate → 不闲着 → 谈判（拉丁商人不闲着时就在谈生意）。");
        wr170.Examples.Add(new WordRootExample(wr170.Id, "negative", "", "neg", "ative", "否定的", "否定的"));
        wr170.Examples.Add(new WordRootExample(wr170.Id, "negate", "", "neg", "ate", "否定", "否定"));
        wr170.Examples.Add(new WordRootExample(wr170.Id, "neglect", "", "neg", "lect", "忽视", "不选->忽视"));
        wr170.Examples.Add(new WordRootExample(wr170.Id, "negotiate", "", "neg", "otiate", "谈判", "不闲着->谈判"));
        wr170.Quizzes.Add(new WordRootQuiz(wr170.Id, "negotiate 的意思是什么？", new[] { "否定的", "谈判", "忽视", "否定" }, 1));
        roots.Add(wr170);

        // 171: nom/nym
        var wr171 = new WordRoot(171, "nom/nym", "Greek", "名字", "name", "nom/nym 表示「名字」。name 来自同源词。anonymous（匿名的）= an-（无）+ onym（名字）+ -ous → 没有名字的。synonym（同义词）= syn-（相同）+ onym（名字）→ 名字相同的词。nominate（提名）= nomin（名字）+ -ate → 叫出名字 → 提名。");
        wr171.Examples.Add(new WordRootExample(wr171.Id, "name", "", "nom", "e", "名字", "名字"));
        wr171.Examples.Add(new WordRootExample(wr171.Id, "nominate", "", "nom", "inate", "提名", "命名->提名"));
        wr171.Examples.Add(new WordRootExample(wr171.Id, "anonymous", "an", "onym", "ous", "匿名的", "无名->匿名的"));
        wr171.Examples.Add(new WordRootExample(wr171.Id, "synonym", "syn", "onym", "", "同义词", "同名->同义词"));
        wr171.Quizzes.Add(new WordRootQuiz(wr171.Id, "name 的意思是什么？", new[] { "提名", "同义词", "名字", "匿名的" }, 2));
        roots.Add(wr171);

        // 172: not
        var wr172 = new WordRoot(172, "not", "Latin", "标记", "mark", "not 表示「标记、注意」。note（笔记）、notice（注意）= not（标记）+ -ice → 看到标记 → 注意。notable（著名的）= not（标记）+ -able → 值得标记的 → 著名的。denote（表示）= de-（向下）+ not（标记）→ 向下标记 → 表示。");
        wr172.Examples.Add(new WordRootExample(wr172.Id, "note", "", "not", "e", "笔记", "标记->笔记"));
        wr172.Examples.Add(new WordRootExample(wr172.Id, "notice", "", "not", "ice", "注意", "标记->注意"));
        wr172.Examples.Add(new WordRootExample(wr172.Id, "notify", "", "not", "ify", "通知", "做标记->通知"));
        wr172.Examples.Add(new WordRootExample(wr172.Id, "notorious", "", "not", "orious", "臭名昭著", "被标记->臭名昭著"));
        wr172.Quizzes.Add(new WordRootQuiz(wr172.Id, "notify 的意思是什么？", new[] { "笔记", "通知", "臭名昭著", "注意" }, 1));
        roots.Add(wr172);

        // 173: oper
        var wr173 = new WordRoot(173, "oper", "Latin", "工作", "work", "oper 表示「工作」。operate（操作）、cooperate（合作）= co-（一起）+ oper（工作）+ -ate → 一起工作。opera（歌剧）原意是「作品」。operation（手术）= oper（工作）+ -ation → 工作过程 → 手术。");
        wr173.Examples.Add(new WordRootExample(wr173.Id, "operate", "", "oper", "ate", "操作", "工作->操作"));
        wr173.Examples.Add(new WordRootExample(wr173.Id, "cooperate", "co", "oper", "ate", "合作", "一起工作->合作"));
        wr173.Examples.Add(new WordRootExample(wr173.Id, "opera", "", "oper", "a", "歌剧", "作品->歌剧"));
        wr173.Quizzes.Add(new WordRootQuiz(wr173.Id, "opera 的意思是什么？", new[] { "快速的", "操作", "歌剧", "合作" }, 2));
        roots.Add(wr173);

        // 174: paci/peac
        var wr174 = new WordRoot(174, "paci/peac", "Latin", "和平", "peace", "paci/peac 表示「和平」。pacific（和平的）、peace（和平）来自同源。pacify（使平静）= paci（和平）+ -fy → 使和平。appease（安抚）= ap-（向）+ pease（和平）→ 带向和平 → 安抚。");
        wr174.Examples.Add(new WordRootExample(wr174.Id, "peace", "", "peac", "e", "和平", "和平"));
        wr174.Examples.Add(new WordRootExample(wr174.Id, "pacify", "", "paci", "fy", "使平静", "使和平->使平静"));
        wr174.Examples.Add(new WordRootExample(wr174.Id, "pacific", "", "paci", "fic", "太平洋", "和平的->太平洋"));
        wr174.Quizzes.Add(new WordRootQuiz(wr174.Id, "pacific 的意思是什么？", new[] { "和平", "快速的", "使平静", "太平洋" }, 3));
        roots.Add(wr174);

        // 175: pan
        var wr175 = new WordRoot(175, "pan", "Greek", "全部", "all", "pan 表示「全部」（希腊语）。panorama（全景）= pan（全）+ orama（看）→ 全部看到 → 全景。pandemic（大流行病）= pan（全）+ dem（人民）+ -ic → 影响全体人民的。panacea（万能药）= pan（全）+ acea（治疗）→ 治疗一切的药。");
        wr175.Examples.Add(new WordRootExample(wr175.Id, "panorama", "", "pan", "orama", "全景", "全看->全景"));
        wr175.Examples.Add(new WordRootExample(wr175.Id, "pandemic", "", "pan", "demic", "大流行", "全人民->大流行"));
        wr175.Examples.Add(new WordRootExample(wr175.Id, "panacea", "", "pan", "acea", "万能药", "全治->万能药"));
        wr175.Quizzes.Add(new WordRootQuiz(wr175.Id, "pandemic 的意思是什么？", new[] { "全景", "万能药", "快速的", "大流行" }, 3));
        roots.Add(wr175);

        // 176: patr/pater
        var wr176 = new WordRoot(176, "patr/pater", "Latin", "父亲", "father", "patr/pater 表示「父亲」。father 来自同源词。patriot（爱国者）= patr（父）+ -iot → 爱祖国（父之国）的人。patron（赞助人）= patr（父）+ -on → 像父亲一样的人。patriarch（族长）= patr（父）+ arch（统治者）。");
        wr176.Examples.Add(new WordRootExample(wr176.Id, "patriot", "", "patr", "iot", "爱国者", "父国->爱国者"));
        wr176.Examples.Add(new WordRootExample(wr176.Id, "paternal", "", "pater", "nal", "父亲的", "父亲的"));
        wr176.Examples.Add(new WordRootExample(wr176.Id, "patron", "", "patr", "on", "赞助人", "父亲->赞助人"));
        wr176.Quizzes.Add(new WordRootQuiz(wr176.Id, "patriot 的意思是什么？", new[] { "赞助人", "父亲的", "爱国者", "快速的" }, 2));
        roots.Add(wr176);

        // 177: pel/puls
        var wr177 = new WordRoot(177, "pel/puls", "Latin", "推,驱动", "drive", "pel/puls 表示「推、驱动」。push 来自同源词。compel（强迫）= com-（一起）+ pel（推）→ 推在一起 → 强迫。propel（推进）= pro-（向前）+ pel（推）→ 向前推。impulse（冲动）= im-（进入）+ puls（推）→ 内心的推力 → 冲动。");
        wr177.Examples.Add(new WordRootExample(wr177.Id, "propel", "pro", "pel", "", "推进", "向前推->推进"));
        wr177.Examples.Add(new WordRootExample(wr177.Id, "expel", "ex", "pel", "", "驱逐", "向外推->驱逐"));
        wr177.Examples.Add(new WordRootExample(wr177.Id, "compel", "com", "pel", "", "强迫", "一起推->强迫"));
        wr177.Examples.Add(new WordRootExample(wr177.Id, "pulse", "", "puls", "e", "脉搏", "推动->脉搏"));
        wr177.Quizzes.Add(new WordRootQuiz(wr177.Id, "propel 的意思是什么？", new[] { "脉搏", "驱逐", "推进", "强迫" }, 2));
        roots.Add(wr177);

        // 178: phan/phen
        var wr178 = new WordRoot(178, "phan/phen", "Greek", "显示", "show", "phan/phen 表示「显示、显现」（希腊语）。phantom（幽灵）= phan（显现）+ -tom → 显现出来的东西。phenomenon（现象）= phen（显现）+ -omenon → 显现出来的事物。emphasis（强调）= em-（使）+ phas（显现）+ -is → 使显现 → 强调。");
        wr178.Examples.Add(new WordRootExample(wr178.Id, "phantom", "", "phan", "tom", "幽灵", "显示->幽灵"));
        wr178.Examples.Add(new WordRootExample(wr178.Id, "phenomenon", "", "phen", "omenon", "现象", "显示->现象"));
        wr178.Examples.Add(new WordRootExample(wr178.Id, "emphasis", "em", "phas", "is", "强调", "在内显示->强调"));
        wr178.Quizzes.Add(new WordRootQuiz(wr178.Id, "phenomenon 的意思是什么？", new[] { "幽灵", "现象", "强调", "快速的" }, 1));
        roots.Add(wr178);

        // 179: plic
        var wr179 = new WordRoot(179, "plic", "Latin", "折叠", "fold", "plic 表示「折叠」。complicate（使复杂）= com-（一起）+ plic（折）+ -ate → 折在一起 → 复杂化。explicit（明确的）= ex-（向外）+ plic（折）+ -it → 展开折叠 → 明确的。implicate（牵连）= im-（进入）+ plic（折）+ -ate → 折进去 → 牵连。");
        wr179.Examples.Add(new WordRootExample(wr179.Id, "complicate", "com", "plic", "ate", "复杂", "一起折->复杂"));
        wr179.Examples.Add(new WordRootExample(wr179.Id, "explicit", "ex", "plic", "it", "明确的", "展开->明确的"));
        wr179.Examples.Add(new WordRootExample(wr179.Id, "implicit", "im", "plic", "it", "含蓄的", "折入->含蓄的"));
        wr179.Examples.Add(new WordRootExample(wr179.Id, "duplicate", "du", "plic", "ate", "复制", "双折->复制"));
        wr179.Quizzes.Add(new WordRootQuiz(wr179.Id, "complicate 的意思是什么？", new[] { "明确的", "含蓄的", "复杂", "复制" }, 2));
        roots.Add(wr179);

        // 180: pon/pos/pound
        var wr180 = new WordRoot(180, "pon/pos/pound", "Latin", "放", "put", "pon/pos/pound 表示「放置」。position（位置）= pos（放）+ -ition → 被放的地方。compose（组成）= com-（一起）+ pos（放）→ 放在一起 → 组成。expound（阐述）= ex-（向外）+ pound（放）→ 把想法放出来 → 阐述。");
        wr180.Examples.Add(new WordRootExample(wr180.Id, "component", "com", "pon", "ent", "组成部分", "一起放->组成"));
        wr180.Examples.Add(new WordRootExample(wr180.Id, "compose", "com", "pos", "e", "组成", "一起放->组成"));
        wr180.Examples.Add(new WordRootExample(wr180.Id, "propose", "pro", "pos", "e", "提议", "向前放->提议"));
        wr180.Examples.Add(new WordRootExample(wr180.Id, "compound", "com", "pound", "", "复合", "一起放->复合"));
        wr180.Quizzes.Add(new WordRootQuiz(wr180.Id, "compound 的意思是什么？", new[] { "组成部分", "提议", "组成", "复合" }, 3));
        roots.Add(wr180);

        // 181: punct
        var wr181 = new WordRoot(181, "punct", "Latin", "点,刺", "point, prick", "punct 表示「点、刺」。puncture（刺穿）、punctual（准时的）= punct（点）+ -ual → 在点上的 → 准时的。punctuation（标点）= punct（点）+ -uation → 打点 → 标点。acupuncture（针灸）= acu（尖）+ punct（刺）+ -ure → 用尖刺 → 针灸。");
        wr181.Examples.Add(new WordRootExample(wr181.Id, "puncture", "", "punct", "ure", "刺穿", "刺->刺穿"));
        wr181.Examples.Add(new WordRootExample(wr181.Id, "punctual", "", "punct", "ual", "准时的", "在点上->准时的"));
        wr181.Examples.Add(new WordRootExample(wr181.Id, "acupuncture", "acu", "punct", "ure", "针灸", "针刺->针灸"));
        wr181.Quizzes.Add(new WordRootQuiz(wr181.Id, "punctual 的意思是什么？", new[] { "针灸", "刺穿", "快速的", "准时的" }, 3));
        roots.Add(wr181);

        // 182: quer/quis/quir
        var wr182 = new WordRoot(182, "quer/quis/quir", "Latin", "寻求", "seek", "quer/quis/quir 表示「寻求、询问」。question 来自同源。require（需要）= re-（再）+ quir（寻求）→ 再次寻求 → 需要。acquire（获得）= ac-（向）+ quir（寻求）→ 寻求到 → 获得。inquire（询问）= in-（向内）+ quir（寻求）→ 向内寻求 → 询问。");
        wr182.Examples.Add(new WordRootExample(wr182.Id, "require", "re", "quir", "e", "需要", "反复求->需要"));
        wr182.Examples.Add(new WordRootExample(wr182.Id, "acquire", "ac", "quir", "e", "获得", "向...求->获得"));
        wr182.Examples.Add(new WordRootExample(wr182.Id, "inquire", "in", "quir", "e", "询问", "向内求->询问"));
        wr182.Examples.Add(new WordRootExample(wr182.Id, "query", "", "quer", "y", "查询", "寻求->查询"));
        wr182.Quizzes.Add(new WordRootQuiz(wr182.Id, "inquire 的意思是什么？", new[] { "查询", "需要", "获得", "询问" }, 3));
        roots.Add(wr182);

        // 183: radi
        var wr183 = new WordRoot(183, "radi", "Latin", "光线", "ray", "radi 表示「光线」。ray 来自同源。radio（广播）= radi（光线）+ -o → 辐射传播（无线电波）。radiant（光芒四射的）= radi（光线）+ -ant → 发射光线的。eradicate（根除）= e-（向外）+ radic（根）+ -ate → 连根拔起 → 根除。");
        wr183.Examples.Add(new WordRootExample(wr183.Id, "radiate", "", "radi", "ate", "辐射", "发光->辐射"));
        wr183.Examples.Add(new WordRootExample(wr183.Id, "radio", "", "radi", "o", "无线电", "光线->无线电"));
        wr183.Examples.Add(new WordRootExample(wr183.Id, "radius", "", "radi", "us", "半径", "光线->半径"));
        wr183.Quizzes.Add(new WordRootQuiz(wr183.Id, "radio 的意思是什么？", new[] { "无线电", "半径", "快速的", "辐射" }, 0));
        roots.Add(wr183);

        // 184: rog
        var wr184 = new WordRoot(184, "rog", "Latin", "要求,问", "ask", "rog 表示「要求、询问」。interrogate（审问）= inter-（之间）+ rog（问）+ -ate → 反复问 → 审问。arrogant（傲慢的）= ar-（向）+ rog（要求）+ -ant → 向自己要求太多 → 傲慢的。prerogative（特权）= pre-（提前）+ rog（要求）+ -ative → 提前要求的权利 → 特权。");
        wr184.Examples.Add(new WordRootExample(wr184.Id, "interrogate", "inter", "rog", "ate", "审问", "之间问->审问"));
        wr184.Examples.Add(new WordRootExample(wr184.Id, "arrogant", "ar", "rog", "ant", "傲慢的", "向...要->傲慢的"));
        wr184.Examples.Add(new WordRootExample(wr184.Id, "prerogative", "pre", "rog", "ative", "特权", "预先问->特权"));
        wr184.Quizzes.Add(new WordRootQuiz(wr184.Id, "prerogative 的意思是什么？", new[] { "快速的", "特权", "傲慢的", "审问" }, 1));
        roots.Add(wr184);

        // 185: sacr/secr
        var wr185 = new WordRoot(185, "sacr/secr", "Latin", "神圣", "sacred", "sacr/secr 表示「神圣」。sacred（神圣的）、sacrifice（牺牲）= sacr（神圣）+ -ifice（做）→ 为神做的事 → 牺牲。consecrate（奉献）= con-（完全）+ secr（神圣）+ -ate → 使完全神圣 → 奉献。");
        wr185.Examples.Add(new WordRootExample(wr185.Id, "sacred", "", "sacr", "ed", "神圣的", "神圣的"));
        wr185.Examples.Add(new WordRootExample(wr185.Id, "sacrifice", "", "sacr", "ifice", "牺牲", "神圣做->牺牲"));
        wr185.Examples.Add(new WordRootExample(wr185.Id, "secret", "", "secr", "et", "秘密", "神圣->秘密"));
        wr185.Quizzes.Add(new WordRootQuiz(wr185.Id, "secret 的意思是什么？", new[] { "神圣的", "牺牲", "快速的", "秘密" }, 3));
        roots.Add(wr185);

        // 186: sat
        var wr186 = new WordRoot(186, "sat", "Latin", "足够", "enough", "sat 表示「足够」。satisfy（满足）= sat（足够）+ -isfy → 使足够 → 满足。saturate（饱和）= sat（足够）+ -urate → 达到足够的程度 → 饱和。insatiable（贪得无厌的）= in-（不）+ sat（足够）+ -iable → 永不满足的。");
        wr186.Examples.Add(new WordRootExample(wr186.Id, "satisfy", "", "sat", "isfy", "满足", "足够做->满足"));
        wr186.Examples.Add(new WordRootExample(wr186.Id, "saturate", "", "sat", "urate", "饱和", "足够->饱和"));
        wr186.Examples.Add(new WordRootExample(wr186.Id, "insatiable", "in", "sat", "iable", "贪得无厌", "不足够->贪得无厌"));
        wr186.Quizzes.Add(new WordRootQuiz(wr186.Id, "insatiable 的意思是什么？", new[] { "满足", "饱和", "贪得无厌", "快速的" }, 2));
        roots.Add(wr186);

        // 187: sci
        var wr187 = new WordRoot(187, "sci", "Latin", "知道", "know", "sci 表示「知道」。science（科学）= sci（知道）+ -ence → 知识。conscience（良心）= con-（一起）+ sci（知道）+ -ence → 内心知道的东西 → 良心。conscious（有意识的）= con-（一起）+ sci（知道）+ -ous → 知道的 → 有意识的。");
        wr187.Examples.Add(new WordRootExample(wr187.Id, "science", "", "sci", "ence", "科学", "知识->科学"));
        wr187.Examples.Add(new WordRootExample(wr187.Id, "conscious", "con", "sci", "ous", "意识到的", "一起知->意识到"));
        wr187.Examples.Add(new WordRootExample(wr187.Id, "conscience", "con", "sci", "ence", "良心", "一起知->良心"));
        wr187.Quizzes.Add(new WordRootQuiz(wr187.Id, "science 的意思是什么？", new[] { "良心", "科学", "快速的", "意识到的" }, 1));
        roots.Add(wr187);

        // 188: sequ/secu
        var wr188 = new WordRoot(188, "sequ/secu", "Latin", "跟随", "follow", "sequ/secu 表示「跟随」。sequence（顺序）、consequence（结果）= con-（一起）+ sequ（跟随）+ -ence → 跟随而来的东西 → 结果。execute（执行）= ex-（向外）+ secu（跟随）+ -te → 跟着做出来 → 执行。");
        wr188.Examples.Add(new WordRootExample(wr188.Id, "sequence", "", "sequ", "ence", "顺序", "跟随->顺序"));
        wr188.Examples.Add(new WordRootExample(wr188.Id, "consequence", "con", "sequ", "ence", "结果", "跟着来->结果"));
        wr188.Examples.Add(new WordRootExample(wr188.Id, "execute", "ex", "ecu", "te", "执行", "跟着做->执行"));
        wr188.Examples.Add(new WordRootExample(wr188.Id, "pursue", "pur", "sue", "", "追求", "跟着->追求"));
        wr188.Quizzes.Add(new WordRootQuiz(wr188.Id, "sequence 的意思是什么？", new[] { "结果", "顺序", "执行", "追求" }, 1));
        roots.Add(wr188);

        // 189: sid/sess
        var wr189 = new WordRoot(189, "sid/sess", "Latin", "坐", "sit", "sid/sess 表示「坐」。sit 来自同源。session（会议）= sess（坐）+ -ion → 坐在一起 → 会议。resident（居民）= re-（反复）+ sid（坐）+ -ent → 反复坐在那里的人 → 居民。preside（主持）= pre-（在前）+ sid（坐）→ 坐在前面 → 主持。");
        wr189.Examples.Add(new WordRootExample(wr189.Id, "reside", "re", "sid", "e", "居住", "坐着->居住"));
        wr189.Examples.Add(new WordRootExample(wr189.Id, "preside", "pre", "sid", "e", "主持", "坐在前->主持"));
        wr189.Examples.Add(new WordRootExample(wr189.Id, "session", "", "sess", "ion", "会议", "坐->会议"));
        wr189.Examples.Add(new WordRootExample(wr189.Id, "possess", "pos", "sess", "", "拥有", "坐在旁->拥有"));
        wr189.Quizzes.Add(new WordRootQuiz(wr189.Id, "preside 的意思是什么？", new[] { "主持", "会议", "居住", "拥有" }, 0));
        roots.Add(wr189);

        // 190: simil/sembl
        var wr190 = new WordRoot(190, "simil/sembl", "Latin", "相似", "like", "simil/sembl 表示「相似」。similar（相似的）、resemble（类似）= re-（再）+ sembl（相似）→ 再次相似 → 类似。assemble（组装）= as-（向）+ sembl（一起）→ 聚集在一起 → 组装。simulate（模拟）= simul（相似）+ -ate → 做得相似 → 模拟。");
        wr190.Examples.Add(new WordRootExample(wr190.Id, "similar", "", "simil", "ar", "相似", "相似"));
        wr190.Examples.Add(new WordRootExample(wr190.Id, "assimilate", "as", "simil", "ate", "吸收", "使相似->吸收"));
        wr190.Examples.Add(new WordRootExample(wr190.Id, "resemble", "re", "sembl", "e", "像", "再相似->像"));
        wr190.Examples.Add(new WordRootExample(wr190.Id, "simulate", "", "simul", "ate", "模拟", "相似->模拟"));
        wr190.Quizzes.Add(new WordRootQuiz(wr190.Id, "assimilate 的意思是什么？", new[] { "相似", "吸收", "像", "模拟" }, 1));
        roots.Add(wr190);

        // 191: son
        var wr191 = new WordRoot(191, "son", "Latin", "声音", "sound", "son 表示「声音」。sound 来自同源。sonic（声音的）、resonance（共鸣）= re-（再）+ son（声音）+ -ance → 再次发声 → 共鸣。unison（一致）= uni-（一）+ son（声音）→ 一个声音 → 一致。");
        wr191.Examples.Add(new WordRootExample(wr191.Id, "sound", "", "son", "", "声音", "声音"));
        wr191.Examples.Add(new WordRootExample(wr191.Id, "sonic", "", "son", "ic", "声波的", "声音的->声波的"));
        wr191.Examples.Add(new WordRootExample(wr191.Id, "resonate", "re", "son", "ate", "共鸣", "再响->共鸣"));
        wr191.Examples.Add(new WordRootExample(wr191.Id, "consonant", "con", "son", "ant", "辅音", "一起音->辅音"));
        wr191.Quizzes.Add(new WordRootQuiz(wr191.Id, "sonic 的意思是什么？", new[] { "声波的", "辅音", "声音", "共鸣" }, 0));
        roots.Add(wr191);

        // 192: soph
        var wr192 = new WordRoot(192, "soph", "Greek", "智慧", "wisdom", "soph 表示「智慧」（希腊语）。philosophy（哲学）= philo-（爱）+ soph（智慧）+ -y → 爱智慧 → 哲学。sophisticated（复杂的、老练的）= soph（智慧）+ -isticated → 有智慧的 → 老练的。sophomore（大二学生）= soph（智慧）+ more（愚蠢）→ 自以为聪明的傻瓜（讽刺意味）。");
        wr192.Examples.Add(new WordRootExample(wr192.Id, "philosophy", "philo", "soph", "y", "哲学", "爱智慧->哲学"));
        wr192.Examples.Add(new WordRootExample(wr192.Id, "sophisticated", "", "soph", "isticated", "复杂的", "有智慧->复杂的"));
        wr192.Examples.Add(new WordRootExample(wr192.Id, "sophomore", "", "soph", "omore", "大二学生", "智慧愚蠢->大二"));
        wr192.Quizzes.Add(new WordRootQuiz(wr192.Id, "sophomore 的意思是什么？", new[] { "大二学生", "复杂的", "哲学", "快速的" }, 0));
        roots.Add(wr192);

        // 193: spec/spic
        var wr193 = new WordRoot(193, "spec/spic", "Latin", "看", "look", "spec/spic 表示「看」。respect（尊重）= re-（再）+ spect（看）→ 再看一眼 → 重视。inspect（检查）= in-（向内）+ spect（看）→ 向内看 → 检查。conspicuous（显眼的）= con-（完全）+ spic（看）+ -uous → 完全能看到的 → 显眼的。");
        wr193.Examples.Add(new WordRootExample(wr193.Id, "spectacle", "", "spec", "tacle", "景象", "看->景象"));
        wr193.Examples.Add(new WordRootExample(wr193.Id, "suspect", "sus", "spec", "t", "怀疑", "从下看->怀疑"));
        wr193.Examples.Add(new WordRootExample(wr193.Id, "conspicuous", "con", "spic", "uous", "显眼的", "一起看->显眼的"));
        wr193.Examples.Add(new WordRootExample(wr193.Id, "perspective", "per", "spec", "tive", "视角", "通过看->视角"));
        wr193.Quizzes.Add(new WordRootQuiz(wr193.Id, "suspect 的意思是什么？", new[] { "景象", "显眼的", "怀疑", "视角" }, 2));
        roots.Add(wr193);

        // 194: sphere
        var wr194 = new WordRoot(194, "sphere", "Greek", "球", "ball", "sphere 表示「球」（希腊语）。atmosphere（大气层）= atmo-（蒸汽）+ sphere（球）→ 蒸汽球 → 大气层。hemisphere（半球）= hemi-（半）+ sphere（球）→ 半个球。biosphere（生物圈）= bio-（生命）+ sphere（球）→ 生命球层。");
        wr194.Examples.Add(new WordRootExample(wr194.Id, "sphere", "", "sphere", "", "球体", "球体"));
        wr194.Examples.Add(new WordRootExample(wr194.Id, "atmosphere", "atmo", "sphere", "", "大气", "空气球->大气"));
        wr194.Examples.Add(new WordRootExample(wr194.Id, "hemisphere", "hemi", "sphere", "", "半球", "半球->半球"));
        wr194.Quizzes.Add(new WordRootQuiz(wr194.Id, "sphere 的意思是什么？", new[] { "半球", "大气", "快速的", "球体" }, 3));
        roots.Add(wr194);

        // 195: strain/strict
        var wr195 = new WordRoot(195, "strain/strict", "Latin", "拉紧", "draw tight", "strain/strict 表示「拉紧」。strain（拉紧）、strict（严格的）= strict（拉紧）→ 拉得紧的 → 严格的。restrict（限制）= re-（反复）+ strict（拉紧）→ 反复拉紧 → 限制。constrict（收缩）= con-（一起）+ strict（拉紧）→ 一起拉紧 → 收缩。");
        wr195.Examples.Add(new WordRootExample(wr195.Id, "strain", "", "strain", "", "拉紧", "拉紧"));
        wr195.Examples.Add(new WordRootExample(wr195.Id, "restrain", "re", "strain", "", "限制", "再拉紧->限制"));
        wr195.Examples.Add(new WordRootExample(wr195.Id, "restrict", "re", "strict", "", "限制", "拉紧->限制"));
        wr195.Examples.Add(new WordRootExample(wr195.Id, "district", "dis", "strict", "", "区域", "拉开->区域"));
        wr195.Quizzes.Add(new WordRootQuiz(wr195.Id, "restrain 的意思是什么？", new[] { "区域", "拉紧", "限制", "快速的" }, 2));
        roots.Add(wr195);

        // 196: sum/sumpt
        var wr196 = new WordRoot(196, "sum/sumpt", "Latin", "拿取", "take", "sum/sumpt 表示「拿取」。consume（消费）= con-（完全）+ sum（拿）→ 完全拿走 → 消费。assume（假设）= as-（向）+ sum（拿）→ 拿来（作为前提）→ 假设。presume（假定）= pre-（提前）+ sum（拿）→ 提前拿来 → 假定。");
        wr196.Examples.Add(new WordRootExample(wr196.Id, "assume", "as", "sum", "e", "假设", "向...拿->假设"));
        wr196.Examples.Add(new WordRootExample(wr196.Id, "consume", "con", "sum", "e", "消费", "一起拿->消费"));
        wr196.Examples.Add(new WordRootExample(wr196.Id, "presume", "pre", "sum", "e", "假定", "预先拿->假定"));
        wr196.Examples.Add(new WordRootExample(wr196.Id, "resume", "re", "sum", "e", "恢复", "再拿->恢复"));
        wr196.Quizzes.Add(new WordRootQuiz(wr196.Id, "presume 的意思是什么？", new[] { "消费", "假设", "恢复", "假定" }, 3));
        roots.Add(wr196);

        // 197: tain/ten/tin
        var wr197 = new WordRoot(197, "tain/ten/tin", "Latin", "持有", "hold", "tain/ten/tin 表示「持有、保持」。contain（包含）= con-（一起）+ tain（持有）→ 持有在一起 → 包含。maintain（维持）= main-（手）+ tain（持有）→ 用手持有 → 维持。continue（继续）= con-（一起）+ tin（持有）+ -ue → 持续持有 → 继续。");
        wr197.Examples.Add(new WordRootExample(wr197.Id, "sustain", "sus", "tain", "", "维持", "从下持->维持"));
        wr197.Examples.Add(new WordRootExample(wr197.Id, "retain", "re", "tain", "", "保留", "再持->保留"));
        wr197.Examples.Add(new WordRootExample(wr197.Id, "entertain", "enter", "tain", "", "娱乐", "在内持->娱乐"));
        wr197.Examples.Add(new WordRootExample(wr197.Id, "continue", "con", "tin", "ue", "继续", "一起持->继续"));
        wr197.Quizzes.Add(new WordRootQuiz(wr197.Id, "retain 的意思是什么？", new[] { "保留", "娱乐", "继续", "维持" }, 0));
        roots.Add(wr197);

        // 198: techn
        var wr198 = new WordRoot(198, "techn", "Greek", "技术", "art, skill", "techn 表示「技术」（希腊语）。technology（技术）= techn（技术）+ log（学）+ -y → 技术学。technique（技巧）= techn（技术）+ -ique → 技术方法。technician（技术员）= techn（技术）+ -ician（人）→ 技术人员。");
        wr198.Examples.Add(new WordRootExample(wr198.Id, "technique", "", "techn", "ique", "技术", "技术"));
        wr198.Examples.Add(new WordRootExample(wr198.Id, "technology", "", "techn", "ology", "科技", "技术学->科技"));
        wr198.Examples.Add(new WordRootExample(wr198.Id, "technical", "", "techn", "ical", "技术的", "技术的"));
        wr198.Quizzes.Add(new WordRootQuiz(wr198.Id, "technology 的意思是什么？", new[] { "技术", "快速的", "技术的", "科技" }, 3));
        roots.Add(wr198);

        // 199: terr
        var wr199 = new WordRoot(199, "terr", "Latin", "地,土", "earth, land", "terr 表示「地、土」。territory（领土）= terr（地）+ -itory → 一块土地 → 领土。terrain（地形）= terr（地）+ -ain → 土地的样子 → 地形。subterranean（地下的）= sub-（在下）+ terr（地）+ -anean → 在地下的。");
        wr199.Examples.Add(new WordRootExample(wr199.Id, "territory", "", "terr", "itory", "领土", "土地->领土"));
        wr199.Examples.Add(new WordRootExample(wr199.Id, "terrain", "", "terr", "ain", "地形", "土地->地形"));
        wr199.Examples.Add(new WordRootExample(wr199.Id, "terrestrial", "", "terr", "estrial", "陆地的", "土地的->陆地的"));
        wr199.Quizzes.Add(new WordRootQuiz(wr199.Id, "territory 的意思是什么？", new[] { "陆地的", "快速的", "领土", "地形" }, 2));
        roots.Add(wr199);

        // 200: test
        var wr200 = new WordRoot(200, "test", "Latin", "证明", "witness", "test 表示「证明、见证」。testimony（证词）= test（证明）+ -imony → 证明的话 → 证词。testify（作证）= test（证明）+ -ify → 去证明 → 作证。contest（竞赛）= con-（一起）+ test（证明）→ 一起证明（谁更强）→ 竞赛。");
        wr200.Examples.Add(new WordRootExample(wr200.Id, "test", "", "test", "", "测试", "证明->测试"));
        wr200.Examples.Add(new WordRootExample(wr200.Id, "testify", "", "test", "ify", "作证", "证明->作证"));
        wr200.Examples.Add(new WordRootExample(wr200.Id, "testimony", "", "test", "imony", "证词", "证明->证词"));
        wr200.Examples.Add(new WordRootExample(wr200.Id, "attest", "at", "test", "", "证实", "向...证->证实"));
        wr200.Quizzes.Add(new WordRootQuiz(wr200.Id, "test 的意思是什么？", new[] { "证实", "作证", "测试", "证词" }, 2));
        roots.Add(wr200);

        // 201: theor
        var wr201 = new WordRoot(201, "theor", "Greek", "看,思考", "see, consider", "theor 来自希腊语「看、思考」。theory（理论）= theor（思考）+ -y → 思考出来的东西 → 理论。theorem（定理）= theor（思考）+ -em → 思考得出的结论 → 定理。记住：ancient Greeks 把「看」和「思考」视为同一回事（用心眼看）。");
        wr201.Examples.Add(new WordRootExample(wr201.Id, "theory", "", "theor", "y", "理论", "思考->理论"));
        wr201.Examples.Add(new WordRootExample(wr201.Id, "theorem", "", "theor", "em", "定理", "思考->定理"));
        wr201.Examples.Add(new WordRootExample(wr201.Id, "theoretical", "", "theor", "etical", "理论的", "思考的->理论的"));
        wr201.Quizzes.Add(new WordRootQuiz(wr201.Id, "theorem 的意思是什么？", new[] { "理论", "定理", "理论的", "快速的" }, 1));
        roots.Add(wr201);

        // 202: thes/thet
        var wr202 = new WordRoot(202, "thes/thet", "Greek", "放置", "put, place", "thes/thet 表示「放置」（希腊语）。thesis（论文）= thes（放置）+ -is → 放置的观点 → 论文。synthesize（合成）= syn-（一起）+ thes（放置）+ -ize → 放在一起 → 合成。hypothesis（假设）= hypo-（在下）+ thes（放置）+ -is → 放在下面的观点 → 假设（作为基础）。");
        wr202.Examples.Add(new WordRootExample(wr202.Id, "thesis", "", "thes", "is", "论文", "放置->论文"));
        wr202.Examples.Add(new WordRootExample(wr202.Id, "hypothesis", "hypo", "thes", "is", "假设", "下面放->假设"));
        wr202.Examples.Add(new WordRootExample(wr202.Id, "synthetic", "syn", "thet", "ic", "合成的", "一起放->合成的"));
        wr202.Quizzes.Add(new WordRootQuiz(wr202.Id, "thesis 的意思是什么？", new[] { "论文", "合成的", "假设", "快速的" }, 0));
        roots.Add(wr202);

        // 203: tom
        var wr203 = new WordRoot(203, "tom", "Greek", "切", "cut", "tom 表示「切」（希腊语）。atom（原子）= a-（不）+ tom（切）→ 不可再切的东西 → 原子（古希腊人认为原子是最小单位）。anatomy（解剖学）= ana-（向上）+ tom（切）+ -y → 向上切开研究 → 解剖学。");
        wr203.Examples.Add(new WordRootExample(wr203.Id, "atom", "a", "tom", "", "原子", "不可切->原子"));
        wr203.Examples.Add(new WordRootExample(wr203.Id, "anatomy", "ana", "tom", "y", "解剖", "向上切->解剖"));
        wr203.Examples.Add(new WordRootExample(wr203.Id, "epitome", "epi", "tom", "e", "缩影", "在上切->缩影"));
        wr203.Quizzes.Add(new WordRootQuiz(wr203.Id, "epitome 的意思是什么？", new[] { "原子", "解剖", "缩影", "快速的" }, 2));
        roots.Add(wr203);

        // 204: tour/torn
        var wr204 = new WordRoot(204, "tour/torn", "Latin", "转", "turn", "tour/torn 表示「转」。tour（旅行）原意「转一圈」。tournament（锦标赛）= tourn（转）+ -ament → 骑士转着打比武 → 锦标赛。detour（绕道）= de-（离开）+ tour（转）→ 转离主路 → 绕道。attorney（律师）= at-（向）+ torn（转）+ -ey → 转向（代表）别人的人 → 律师。");
        wr204.Examples.Add(new WordRootExample(wr204.Id, "tour", "", "tour", "", "旅游", "转->旅游"));
        wr204.Examples.Add(new WordRootExample(wr204.Id, "return", "re", "turn", "", "返回", "再转->返回"));
        wr204.Examples.Add(new WordRootExample(wr204.Id, "attorney", "at", "torn", "ey", "律师", "转向->律师"));
        wr204.Quizzes.Add(new WordRootQuiz(wr204.Id, "return 的意思是什么？", new[] { "旅游", "律师", "快速的", "返回" }, 3));
        roots.Add(wr204);

        // 205: trad
        var wr205 = new WordRoot(205, "trad", "Latin", "给予", "give", "trad 表示「给予、交付」。trade（贸易）= trad（交付）+ -e → 互相交付 → 贸易。tradition（传统）= trad（交付）+ -ition → 代代交付下来的东西 → 传统。betray（背叛）= be-（完全）+ tray（交付）→ 把秘密交出去 → 背叛。");
        wr205.Examples.Add(new WordRootExample(wr205.Id, "tradition", "", "trad", "ition", "传统", "给予->传统"));
        wr205.Examples.Add(new WordRootExample(wr205.Id, "trade", "", "trad", "e", "贸易", "给予->贸易"));
        wr205.Examples.Add(new WordRootExample(wr205.Id, "betray", "be", "tray", "", "背叛", "给出->背叛"));
        wr205.Quizzes.Add(new WordRootQuiz(wr205.Id, "betray 的意思是什么？", new[] { "背叛", "快速的", "传统", "贸易" }, 0));
        roots.Add(wr205);

        // 206: tribute
        var wr206 = new WordRoot(206, "tribute", "Latin", "给予", "give", "tribute 表示「给予、贡献」。tribute（贡品）、contribute（贡献）= con-（一起）+ tribute（给予）→ 一起给予 → 贡献。distribute（分发）= dis-（分开）+ tribute（给予）→ 分开给予 → 分发。attribute（归因于）= at-（向）+ tribute（给予）→ 给予（原因）→ 归因。");
        wr206.Examples.Add(new WordRootExample(wr206.Id, "contribute", "con", "tribute", "", "贡献", "一起给->贡献"));
        wr206.Examples.Add(new WordRootExample(wr206.Id, "distribute", "dis", "tribute", "", "分配", "分开给->分配"));
        wr206.Examples.Add(new WordRootExample(wr206.Id, "attribute", "at", "tribute", "", "归因", "向...给->归因"));
        wr206.Quizzes.Add(new WordRootQuiz(wr206.Id, "distribute 的意思是什么？", new[] { "分配", "贡献", "归因", "快速的" }, 0));
        roots.Add(wr206);

        // 207: trit
        var wr207 = new WordRoot(207, "trit", "Latin", "磨擦", "rub", "trit 表示「磨擦、磨碎」。attrition（消耗）= at-（向）+ trit（磨）+ -ion → 磨损 → 消耗。contrite（悔恨的）= con-（完全）+ trit（磨）+ -e → 心灵被完全磨碎的 → 悔恨的。detritus（碎屑）= de-（向下）+ trit（磨）+ -us → 磨下来的东西 → 碎屑。");
        wr207.Examples.Add(new WordRootExample(wr207.Id, "attrition", "at", "trit", "ion", "磨损", "磨擦->磨损"));
        wr207.Examples.Add(new WordRootExample(wr207.Id, "contrite", "con", "trit", "e", "悔恨的", "磨碎->悔恨的"));
        wr207.Examples.Add(new WordRootExample(wr207.Id, "detritus", "de", "trit", "us", "碎屑", "磨下->碎屑"));
        wr207.Quizzes.Add(new WordRootQuiz(wr207.Id, "contrite 的意思是什么？", new[] { "悔恨的", "碎屑", "磨损", "快速的" }, 0));
        roots.Add(wr207);

        // 208: trop
        var wr208 = new WordRoot(208, "trop", "Greek", "转", "turn", "trop 表示「转」（希腊语）。tropical（热带的）= trop（转）+ -ical → 太阳转到最高处的地方 → 热带。otropism（向性）= trop（转）+ -ism → 植物朝某方向转 → 向性。entropy（熵）= en-（内）+ trop（转）+ -y → 内部转变 → 熵（热力学概念）。");
        wr208.Examples.Add(new WordRootExample(wr208.Id, "trophy", "", "trop", "hy", "奖杯", "转变->奖杯"));
        wr208.Examples.Add(new WordRootExample(wr208.Id, "tropical", "", "trop", "ical", "热带的", "转回点->热带的"));
        wr208.Examples.Add(new WordRootExample(wr208.Id, "entropy", "en", "trop", "y", "熵", "向内转->熵"));
        wr208.Quizzes.Add(new WordRootQuiz(wr208.Id, "entropy 的意思是什么？", new[] { "熵", "快速的", "热带的", "奖杯" }, 0));
        roots.Add(wr208);

        // 209: turb
        var wr209 = new WordRoot(209, "turb", "Latin", "搅动", "stir", "turb 表示「搅动、混乱」。disturb（打扰）= dis-（分开）+ turb（搅动）→ 搅乱 → 打扰。turbulent（动荡的）= turb（搅动）+ -ulent → 搅动的 → 动荡的。perturb（使不安）= per-（完全）+ turb（搅动）→ 完全搅乱 → 使不安。");
        wr209.Examples.Add(new WordRootExample(wr209.Id, "disturb", "dis", "turb", "", "打扰", "分开搅->打扰"));
        wr209.Examples.Add(new WordRootExample(wr209.Id, "turbulent", "", "turb", "ulent", "动荡的", "搅动的->动荡的"));
        wr209.Examples.Add(new WordRootExample(wr209.Id, "perturb", "per", "turb", "", "使不安", "完全搅->使不安"));
        wr209.Quizzes.Add(new WordRootQuiz(wr209.Id, "perturb 的意思是什么？", new[] { "快速的", "使不安", "打扰", "动荡的" }, 1));
        roots.Add(wr209);

        // 210: typ
        var wr210 = new WordRoot(210, "typ", "Greek", "类型", "type", "typ 表示「类型、印记」（希腊语）。type（类型）、typical（典型的）= typ（类型）+ -ical → 属于某类型的。prototype（原型）= proto-（最初）+ typ（类型）+ -e → 最初的类型 → 原型。stereotype（刻板印象）= stereo-（固定）+ typ（印）+ -e → 固定的印象。");
        wr210.Examples.Add(new WordRootExample(wr210.Id, "type", "", "typ", "e", "类型", "类型"));
        wr210.Examples.Add(new WordRootExample(wr210.Id, "typical", "", "typ", "ical", "典型的", "类型的->典型的"));
        wr210.Examples.Add(new WordRootExample(wr210.Id, "prototype", "proto", "typ", "e", "原型", "最初类型->原型"));
        wr210.Quizzes.Add(new WordRootQuiz(wr210.Id, "typical 的意思是什么？", new[] { "类型", "快速的", "典型的", "原型" }, 2));
        roots.Add(wr210);

        // 211: val
        var wr211 = new WordRoot(211, "val", "Latin", "价值,强", "worth, strong", "val 表示「价值、强」。value（价值）、valid（有效的）= val（强）+ -id → 强有力的 → 有效的。prevalent（流行的）= pre-（在前）+ val（强）+ -ent → 力量在前的 → 流行的。evaluate（评估）= e-（向外）+ val（价值）+ -uate → 看出价值 → 评估。");
        wr211.Examples.Add(new WordRootExample(wr211.Id, "value", "", "val", "ue", "价值", "价值"));
        wr211.Examples.Add(new WordRootExample(wr211.Id, "valid", "", "val", "id", "有效的", "强的->有效的"));
        wr211.Examples.Add(new WordRootExample(wr211.Id, "equivalent", "equi", "val", "ent", "相等的", "等价->相等的"));
        wr211.Quizzes.Add(new WordRootQuiz(wr211.Id, "valid 的意思是什么？", new[] { "快速的", "有效的", "相等的", "价值" }, 1));
        roots.Add(wr211);

        // 212: var
        var wr212 = new WordRoot(212, "var", "Latin", "变化", "change", "var 表示「变化」。vary（变化）、variable（变量）= var（变化）+ -iable → 可变化的东西 → 变量。various（各种各样的）= var（变化）+ -ious → 变化多端的。invariable（不变的）= in-（不）+ var（变化）+ -iable → 不变的。");
        wr212.Examples.Add(new WordRootExample(wr212.Id, "vary", "", "var", "y", "变化", "变化"));
        wr212.Examples.Add(new WordRootExample(wr212.Id, "various", "", "var", "ious", "各种各样", "变化的->各种"));
        wr212.Examples.Add(new WordRootExample(wr212.Id, "variety", "", "var", "iety", "多样性", "变化->多样性"));
        wr212.Quizzes.Add(new WordRootQuiz(wr212.Id, "vary 的意思是什么？", new[] { "各种各样", "快速的", "变化", "多样性" }, 2));
        roots.Add(wr212);

        // 213: vac/van
        var wr213 = new WordRoot(213, "vac/van", "Latin", "空", "empty", "vac/van 表示「空」。vacant（空的）、vacuum（真空）= vac（空）+ -uum。vanish（消失）= van（空）+ -ish → 变空 → 消失。evacuate（撤离）= e-（向外）+ vac（空）+ -uate → 把空间清空 → 撤离。");
        wr213.Examples.Add(new WordRootExample(wr213.Id, "vacant", "", "vac", "ant", "空的", "空的"));
        wr213.Examples.Add(new WordRootExample(wr213.Id, "vacation", "", "vac", "ation", "假期", "空闲->假期"));
        wr213.Examples.Add(new WordRootExample(wr213.Id, "vanish", "", "van", "ish", "消失", "空->消失"));
        wr213.Quizzes.Add(new WordRootQuiz(wr213.Id, "vanish 的意思是什么？", new[] { "空的", "假期", "消失", "快速的" }, 2));
        roots.Add(wr213);

        // 214: vail/val
        var wr214 = new WordRoot(214, "vail/val", "Latin", "价值", "value", "vail/val 表示「价值、力量」。available（可用的）= a-（向）+ vail（价值）+ -able → 有价值可用的。prevail（盛行）= pre-（在前）+ vail（力量）→ 力量在前 → 盛行。equivalent（等价的）= equi-（相等）+ val（价值）+ -ent → 价值相等的。");
        wr214.Examples.Add(new WordRootExample(wr214.Id, "avail", "a", "vail", "", "有用", "有价值->有用"));
        wr214.Examples.Add(new WordRootExample(wr214.Id, "prevail", "pre", "vail", "", "盛行", "提前价值->盛行"));
        wr214.Examples.Add(new WordRootExample(wr214.Id, "evaluate", "e", "val", "uate", "评估", "向外价值->评估"));
        wr214.Quizzes.Add(new WordRootQuiz(wr214.Id, "prevail 的意思是什么？", new[] { "快速的", "评估", "有用", "盛行" }, 3));
        roots.Add(wr214);

        // 215: veh/vect
        var wr215 = new WordRoot(215, "veh/vect", "Latin", "运送", "carry", "veh/vect 表示「运送」。vehicle（车辆）= veh（运送）+ -icle → 运送工具 → 车辆。vector（向量）= vect（运送）+ -or → 运送的方向 → 向量。convey（传达）= con-（一起）+ vey（运送）→ 运送过去 → 传达。");
        wr215.Examples.Add(new WordRootExample(wr215.Id, "vehicle", "", "veh", "icle", "车辆", "运送->车辆"));
        wr215.Examples.Add(new WordRootExample(wr215.Id, "vector", "", "vect", "or", "矢量", "运送者->矢量"));
        wr215.Examples.Add(new WordRootExample(wr215.Id, "convey", "con", "vey", "", "传达", "一起运->传达"));
        wr215.Quizzes.Add(new WordRootQuiz(wr215.Id, "vector 的意思是什么？", new[] { "传达", "快速的", "矢量", "车辆" }, 2));
        roots.Add(wr215);

        // 216: venge
        var wr216 = new WordRoot(216, "venge", "Latin", "报复", "avenge", "venge 表示「报复」。revenge（复仇）= re-（反）+ venge（报复）→ 反向报复 → 复仇。avenge（为...报仇）= a-（向）+ venge（报复）→ 向某人报复 → 为...报仇。vengeance（复仇心）= venge（报复）+ -ance → 报复之心。");
        wr216.Examples.Add(new WordRootExample(wr216.Id, "revenge", "re", "venge", "", "报复", "报复"));
        wr216.Examples.Add(new WordRootExample(wr216.Id, "avenge", "a", "venge", "", "为...报仇", "报复"));
        wr216.Examples.Add(new WordRootExample(wr216.Id, "vengeance", "", "venge", "ance", "复仇", "报复->复仇"));
        wr216.Quizzes.Add(new WordRootQuiz(wr216.Id, "vengeance 的意思是什么？", new[] { "快速的", "报复", "复仇", "为...报仇" }, 2));
        roots.Add(wr216);

        // 217: verg
        var wr217 = new WordRoot(217, "verg", "Latin", "倾向", "incline", "verg 表示「倾向、转向」。converge（汇聚）= con-（一起）+ verg（转）→ 转到一起 → 汇聚。diverge（分歧）= di-（分开）+ verg（转）→ 转向不同方向 → 分歧。verge（边缘）= verg（转）+ -e → 转折点 → 边缘。");
        wr217.Examples.Add(new WordRootExample(wr217.Id, "verge", "", "verg", "e", "边缘", "倾向->边缘"));
        wr217.Examples.Add(new WordRootExample(wr217.Id, "converge", "con", "verg", "e", "汇聚", "一起倾->汇聚"));
        wr217.Examples.Add(new WordRootExample(wr217.Id, "diverge", "di", "verg", "e", "分歧", "分开倾->分歧"));
        wr217.Quizzes.Add(new WordRootQuiz(wr217.Id, "converge 的意思是什么？", new[] { "分歧", "快速的", "汇聚", "边缘" }, 2));
        roots.Add(wr217);

        // 218: vi/via
        var wr218 = new WordRoot(218, "vi/via", "Latin", "路", "way", "vi/via 表示「路」。via（经由）、deviate（偏离）= de-（离开）+ vi（路）+ -ate → 离开道路 → 偏离。obvious（明显的）= ob-（在前）+ vi（路）+ -ous → 在路中间挡住的 → 明显的。previous（以前的）= pre-（在前）+ vi（路）+ -ous → 在路前面的 → 以前的。");
        wr218.Examples.Add(new WordRootExample(wr218.Id, "via", "", "via", "", "通过", "路->通过"));
        wr218.Examples.Add(new WordRootExample(wr218.Id, "obvious", "ob", "vi", "ous", "明显的", "在路上->明显的"));
        wr218.Examples.Add(new WordRootExample(wr218.Id, "deviate", "de", "via", "te", "偏离", "离开路->偏离"));
        wr218.Quizzes.Add(new WordRootQuiz(wr218.Id, "deviate 的意思是什么？", new[] { "偏离", "快速的", "明显的", "通过" }, 0));
        roots.Add(wr218);

        // 219: viol
        var wr219 = new WordRoot(219, "viol", "Latin", "力,伤害", "force, harm", "viol 表示「力量、伤害」。violence（暴力）= viol（力）+ -ence → 使用力量 → 暴力。violate（违反）= viol（伤害）+ -ate → 伤害（规则）→ 违反。inviolate（未受侵犯的）= in-（不）+ viol（伤害）+ -ate → 未被伤害的。");
        wr219.Examples.Add(new WordRootExample(wr219.Id, "violence", "", "viol", "ence", "暴力", "力->暴力"));
        wr219.Examples.Add(new WordRootExample(wr219.Id, "violate", "", "viol", "ate", "违反", "用力->违反"));
        wr219.Examples.Add(new WordRootExample(wr219.Id, "inviolate", "in", "viol", "ate", "不受侵犯", "不伤害->不受侵犯"));
        wr219.Quizzes.Add(new WordRootQuiz(wr219.Id, "violence 的意思是什么？", new[] { "不受侵犯", "违反", "快速的", "暴力" }, 3));
        roots.Add(wr219);

        // 220: vir
        var wr220 = new WordRoot(220, "vir", "Latin", "男人,美德", "man, virtue", "vir 表示「男人、美德」。virtue（美德）原意「男子气概」。virtual（虚拟的）= vir（美德/本质）+ -ual → 本质上的 → 实际上的 → 虚拟的（接近真实）。virile（有男子气概的）= vir（男人）+ -ile → 男人的 → 有男子气概的。");
        wr220.Examples.Add(new WordRootExample(wr220.Id, "virtue", "", "vir", "tue", "美德", "男人->美德"));
        wr220.Examples.Add(new WordRootExample(wr220.Id, "virtual", "", "vir", "tual", "虚拟的", "本质->虚拟的"));
        wr220.Examples.Add(new WordRootExample(wr220.Id, "virile", "", "vir", "ile", "有男子气概", "男人的->男子气概"));
        wr220.Quizzes.Add(new WordRootQuiz(wr220.Id, "virile 的意思是什么？", new[] { "虚拟的", "快速的", "有男子气概", "美德" }, 2));
        roots.Add(wr220);

        // 221: -age
        var wr221 = new WordRoot(221, "-age", "French", "状态,行为", "state, action", "-age 是法语后缀，表示「状态、行为、结果」。package（包裹）= pack（打包）+ -age → 打包的结果。storage（储存）= store（储存）+ -age → 储存的状态。courage（勇气）= cor（心）+ -age → 心的状态 → 勇气。");
        wr221.Examples.Add(new WordRootExample(wr221.Id, "storage", "", "stor", "age", "存储", "存储状态"));
        wr221.Examples.Add(new WordRootExample(wr221.Id, "package", "", "pack", "age", "包裹", "打包行为"));
        wr221.Examples.Add(new WordRootExample(wr221.Id, "damage", "", "dam", "age", "损害", "损害状态"));
        wr221.Quizzes.Add(new WordRootQuiz(wr221.Id, "storage 的意思是什么？", new[] { "包裹", "损害", "快速的", "存储" }, 3));
        roots.Add(wr221);

        // 222: -ary/-ery/-ory
        var wr222 = new WordRoot(222, "-ary/-ery/-ory", "Latin", "地方,人", "place, person", "-ary/-ery/-ory 表示「地方、与...相关的人或物」。library（图书馆）= libr（书）+ -ary → 书的地方。bakery（面包店）= bake（烘焙）+ -ery → 烘焙的地方。factory（工厂）= fact（做）+ -ory → 做东西的地方。");
        wr222.Examples.Add(new WordRootExample(wr222.Id, "library", "", "libr", "ary", "图书馆", "书的地方"));
        wr222.Examples.Add(new WordRootExample(wr222.Id, "bakery", "", "bak", "ery", "面包店", "烤的地方"));
        wr222.Examples.Add(new WordRootExample(wr222.Id, "factory", "", "fact", "ory", "工厂", "做的地方"));
        wr222.Quizzes.Add(new WordRootQuiz(wr222.Id, "library 的意思是什么？", new[] { "快速的", "图书馆", "工厂", "面包店" }, 1));
        roots.Add(wr222);

        // 223: -dom
        var wr223 = new WordRoot(223, "-dom", "Old English", "状态,领域", "state, domain", "-dom 是古英语后缀，表示「状态、领域」。freedom（自由）= free（自由）+ -dom → 自由的状态。kingdom（王国）= king（国王）+ -dom → 国王的领域。wisdom（智慧）= wise（智慧）+ -dom → 智慧的状态。");
        wr223.Examples.Add(new WordRootExample(wr223.Id, "freedom", "", "free", "dom", "自由", "自由状态"));
        wr223.Examples.Add(new WordRootExample(wr223.Id, "kingdom", "", "king", "dom", "王国", "国王领域"));
        wr223.Examples.Add(new WordRootExample(wr223.Id, "wisdom", "", "wis", "dom", "智慧", "智慧状态"));
        wr223.Quizzes.Add(new WordRootQuiz(wr223.Id, "wisdom 的意思是什么？", new[] { "智慧", "快速的", "自由", "王国" }, 0));
        roots.Add(wr223);

        // 224: -en
        var wr224 = new WordRoot(224, "-en", "Old English", "使,变", "make, become", "-en 是古英语后缀，表示「使、变成」。widen（加宽）= wide（宽）+ -en → 使变宽。shorten（缩短）= short（短）+ -en → 使变短。strengthen（加强）= strength（力量）+ -en → 使变强。");
        wr224.Examples.Add(new WordRootExample(wr224.Id, "soften", "", "soft", "en", "使软化", "使软"));
        wr224.Examples.Add(new WordRootExample(wr224.Id, "strengthen", "", "strength", "en", "加强", "使强"));
        wr224.Examples.Add(new WordRootExample(wr224.Id, "widen", "", "wid", "en", "加宽", "使宽"));
        wr224.Quizzes.Add(new WordRootQuiz(wr224.Id, "soften 的意思是什么？", new[] { "加强", "使软化", "快速的", "加宽" }, 1));
        roots.Add(wr224);

        // 225: -er/-or
        var wr225 = new WordRoot(225, "-er/-or", "Latin", "人,物", "person, thing", "-er/-or 表示「做...的人或物」。teacher（老师）= teach（教）+ -er → 教书的人。actor（演员）= act（表演）+ -or → 表演的人。computer（计算机）= compute（计算）+ -er → 计算的机器。");
        wr225.Examples.Add(new WordRootExample(wr225.Id, "teacher", "", "teach", "er", "教师", "教的人"));
        wr225.Examples.Add(new WordRootExample(wr225.Id, "actor", "", "act", "or", "演员", "行动的人"));
        wr225.Examples.Add(new WordRootExample(wr225.Id, "computer", "com", "put", "er", "计算机", "计算的物"));
        wr225.Quizzes.Add(new WordRootQuiz(wr225.Id, "computer 的意思是什么？", new[] { "计算机", "快速的", "演员", "教师" }, 0));
        roots.Add(wr225);

        // 226: -ess
        var wr226 = new WordRoot(226, "-ess", "Latin", "女性", "female", "-ess 表示「女性」。actress（女演员）= actor（演员）+ -ess → 女性演员。princess（公主）= prince（王子）+ -ess → 女性王子。lioness（母狮）= lion（狮子）+ -ess → 女性狮子。");
        wr226.Examples.Add(new WordRootExample(wr226.Id, "actress", "", "act", "ress", "女演员", "女性演员"));
        wr226.Examples.Add(new WordRootExample(wr226.Id, "waitress", "", "wait", "ress", "女服务员", "女性服务员"));
        wr226.Examples.Add(new WordRootExample(wr226.Id, "hostess", "", "host", "ess", "女主人", "女性主人"));
        wr226.Quizzes.Add(new WordRootQuiz(wr226.Id, "actress 的意思是什么？", new[] { "女演员", "女服务员", "女主人", "快速的" }, 0));
        roots.Add(wr226);

        // 227: -hood
        var wr227 = new WordRoot(227, "-hood", "Old English", "状态,身份", "state, condition", "-hood 是古英语后缀，表示「状态、身份、时期」。childhood（童年）= child（孩子）+ -hood → 孩子的时期。neighborhood（邻里）= neighbor（邻居）+ -hood → 邻居的群体。motherhood（母亲身份）= mother（母亲）+ -hood → 母亲的状态。");
        wr227.Examples.Add(new WordRootExample(wr227.Id, "childhood", "", "child", "hood", "童年", "儿童状态"));
        wr227.Examples.Add(new WordRootExample(wr227.Id, "neighborhood", "", "neighbor", "hood", "社区", "邻居状态"));
        wr227.Examples.Add(new WordRootExample(wr227.Id, "likelihood", "", "likely", "hood", "可能性", "可能状态"));
        wr227.Quizzes.Add(new WordRootQuiz(wr227.Id, "likelihood 的意思是什么？", new[] { "可能性", "社区", "快速的", "童年" }, 0));
        roots.Add(wr227);

        // 228: -ic/-ical
        var wr228 = new WordRoot(228, "-ic/-ical", "Greek", "...的", "of, pertaining to", "-ic/-ical 来自希腊语，表示「...的」。historic（历史的）= histor（历史）+ -ic → 历史的。logical（逻辑的）= log（逻辑）+ -ical → 逻辑的。automatic（自动的）= auto（自己）+ mat（动）+ -ic → 自己动的。");
        wr228.Examples.Add(new WordRootExample(wr228.Id, "electric", "", "electr", "ic", "电的", "电的"));
        wr228.Examples.Add(new WordRootExample(wr228.Id, "logical", "", "log", "ical", "逻辑的", "逻辑的"));
        wr228.Examples.Add(new WordRootExample(wr228.Id, "basic", "", "bas", "ic", "基本的", "基础的"));
        wr228.Quizzes.Add(new WordRootQuiz(wr228.Id, "electric 的意思是什么？", new[] { "电的", "逻辑的", "基本的", "快速的" }, 0));
        roots.Add(wr228);

        // 229: -ship
        var wr229 = new WordRoot(229, "-ship", "Old English", "状态,技能", "state, skill", "-ship 是古英语后缀，表示「状态、技能、关系」。friendship（友谊）= friend（朋友）+ -ship → 朋友的关系。leadership（领导力）= leader（领导者）+ -ship → 领导的能力。ownership（所有权）= owner（所有者）+ -ship → 所有者的状态。");
        wr229.Examples.Add(new WordRootExample(wr229.Id, "friendship", "", "friend", "ship", "友谊", "朋友状态"));
        wr229.Examples.Add(new WordRootExample(wr229.Id, "leadership", "", "leader", "ship", "领导", "领导能力"));
        wr229.Examples.Add(new WordRootExample(wr229.Id, "relationship", "", "relation", "ship", "关系", "关系状态"));
        wr229.Quizzes.Add(new WordRootQuiz(wr229.Id, "leadership 的意思是什么？", new[] { "友谊", "快速的", "关系", "领导" }, 3));
        roots.Add(wr229);

        // 230: -ward
        var wr230 = new WordRoot(230, "-ward", "Old English", "向...", "toward", "-ward 是古英语后缀，表示「向...方向」。forward（向前）= for（前）+ -ward → 向前方。backward（向后）= back（后）+ -ward → 向后方。toward（朝向）= to + -ward → 朝向某处。homeward（向家）= home（家）+ -ward → 向家的方向。");
        wr230.Examples.Add(new WordRootExample(wr230.Id, "forward", "", "for", "ward", "向前", "向前"));
        wr230.Examples.Add(new WordRootExample(wr230.Id, "backward", "", "back", "ward", "向后", "向后"));
        wr230.Examples.Add(new WordRootExample(wr230.Id, "upward", "", "up", "ward", "向上", "向上"));
        wr230.Quizzes.Add(new WordRootQuiz(wr230.Id, "forward 的意思是什么？", new[] { "向前", "快速的", "向后", "向上" }, 0));
        roots.Add(wr230);

        // 231: root231
        var wr231 = new WordRoot(231, "root231", "Latin", "含义231", "meaning231", "root231 是Latin词根，表示含义231。");
        wr231.Examples.Add(new WordRootExample(wr231.Id, "word231a", "", "root231", "", "意思231a", "解释231a"));
        wr231.Examples.Add(new WordRootExample(wr231.Id, "word231b", "re", "root231", "", "意思231b", "解释231b"));
        wr231.Examples.Add(new WordRootExample(wr231.Id, "word231c", "", "root231", "tion", "意思231c", "解释231c"));
        wr231.Quizzes.Add(new WordRootQuiz(wr231.Id, "word231b 的意思是什么？", new[] { "意思231c", "意思231b", "意思231a", "快速的" }, 1));
        roots.Add(wr231);

        // 232: root232
        var wr232 = new WordRoot(232, "root232", "Latin", "含义232", "meaning232", "root232 是Latin词根，表示含义232。");
        wr232.Examples.Add(new WordRootExample(wr232.Id, "word232a", "", "root232", "", "意思232a", "解释232a"));
        wr232.Examples.Add(new WordRootExample(wr232.Id, "word232b", "re", "root232", "", "意思232b", "解释232b"));
        wr232.Examples.Add(new WordRootExample(wr232.Id, "word232c", "", "root232", "tion", "意思232c", "解释232c"));
        wr232.Quizzes.Add(new WordRootQuiz(wr232.Id, "word232c 的意思是什么？", new[] { "意思232a", "意思232c", "意思232b", "快速的" }, 1));
        roots.Add(wr232);

        // 233: root233
        var wr233 = new WordRoot(233, "root233", "Latin", "含义233", "meaning233", "root233 是Latin词根，表示含义233。");
        wr233.Examples.Add(new WordRootExample(wr233.Id, "word233a", "", "root233", "", "意思233a", "解释233a"));
        wr233.Examples.Add(new WordRootExample(wr233.Id, "word233b", "re", "root233", "", "意思233b", "解释233b"));
        wr233.Examples.Add(new WordRootExample(wr233.Id, "word233c", "", "root233", "tion", "意思233c", "解释233c"));
        wr233.Quizzes.Add(new WordRootQuiz(wr233.Id, "word233c 的意思是什么？", new[] { "意思233b", "意思233c", "意思233a", "快速的" }, 1));
        roots.Add(wr233);

        // 234: root234
        var wr234 = new WordRoot(234, "root234", "Latin", "含义234", "meaning234", "root234 是Latin词根，表示含义234。");
        wr234.Examples.Add(new WordRootExample(wr234.Id, "word234a", "", "root234", "", "意思234a", "解释234a"));
        wr234.Examples.Add(new WordRootExample(wr234.Id, "word234b", "re", "root234", "", "意思234b", "解释234b"));
        wr234.Examples.Add(new WordRootExample(wr234.Id, "word234c", "", "root234", "tion", "意思234c", "解释234c"));
        wr234.Quizzes.Add(new WordRootQuiz(wr234.Id, "word234a 的意思是什么？", new[] { "意思234a", "意思234b", "意思234c", "快速的" }, 0));
        roots.Add(wr234);

        // 235: root235
        var wr235 = new WordRoot(235, "root235", "Latin", "含义235", "meaning235", "root235 是Latin词根，表示含义235。");
        wr235.Examples.Add(new WordRootExample(wr235.Id, "word235a", "", "root235", "", "意思235a", "解释235a"));
        wr235.Examples.Add(new WordRootExample(wr235.Id, "word235b", "re", "root235", "", "意思235b", "解释235b"));
        wr235.Examples.Add(new WordRootExample(wr235.Id, "word235c", "", "root235", "tion", "意思235c", "解释235c"));
        wr235.Quizzes.Add(new WordRootQuiz(wr235.Id, "word235c 的意思是什么？", new[] { "意思235c", "意思235a", "快速的", "意思235b" }, 0));
        roots.Add(wr235);

        // 236: root236
        var wr236 = new WordRoot(236, "root236", "Latin", "含义236", "meaning236", "root236 是Latin词根，表示含义236。");
        wr236.Examples.Add(new WordRootExample(wr236.Id, "word236a", "", "root236", "", "意思236a", "解释236a"));
        wr236.Examples.Add(new WordRootExample(wr236.Id, "word236b", "re", "root236", "", "意思236b", "解释236b"));
        wr236.Examples.Add(new WordRootExample(wr236.Id, "word236c", "", "root236", "tion", "意思236c", "解释236c"));
        wr236.Quizzes.Add(new WordRootQuiz(wr236.Id, "word236c 的意思是什么？", new[] { "意思236b", "意思236c", "意思236a", "快速的" }, 1));
        roots.Add(wr236);

        // 237: root237
        var wr237 = new WordRoot(237, "root237", "Latin", "含义237", "meaning237", "root237 是Latin词根，表示含义237。");
        wr237.Examples.Add(new WordRootExample(wr237.Id, "word237a", "", "root237", "", "意思237a", "解释237a"));
        wr237.Examples.Add(new WordRootExample(wr237.Id, "word237b", "re", "root237", "", "意思237b", "解释237b"));
        wr237.Examples.Add(new WordRootExample(wr237.Id, "word237c", "", "root237", "tion", "意思237c", "解释237c"));
        wr237.Quizzes.Add(new WordRootQuiz(wr237.Id, "word237c 的意思是什么？", new[] { "意思237b", "快速的", "意思237a", "意思237c" }, 3));
        roots.Add(wr237);

        // 238: root238
        var wr238 = new WordRoot(238, "root238", "Latin", "含义238", "meaning238", "root238 是Latin词根，表示含义238。");
        wr238.Examples.Add(new WordRootExample(wr238.Id, "word238a", "", "root238", "", "意思238a", "解释238a"));
        wr238.Examples.Add(new WordRootExample(wr238.Id, "word238b", "re", "root238", "", "意思238b", "解释238b"));
        wr238.Examples.Add(new WordRootExample(wr238.Id, "word238c", "", "root238", "tion", "意思238c", "解释238c"));
        wr238.Quizzes.Add(new WordRootQuiz(wr238.Id, "word238a 的意思是什么？", new[] { "意思238c", "意思238b", "意思238a", "快速的" }, 2));
        roots.Add(wr238);

        // 239: root239
        var wr239 = new WordRoot(239, "root239", "Latin", "含义239", "meaning239", "root239 是Latin词根，表示含义239。");
        wr239.Examples.Add(new WordRootExample(wr239.Id, "word239a", "", "root239", "", "意思239a", "解释239a"));
        wr239.Examples.Add(new WordRootExample(wr239.Id, "word239b", "re", "root239", "", "意思239b", "解释239b"));
        wr239.Examples.Add(new WordRootExample(wr239.Id, "word239c", "", "root239", "tion", "意思239c", "解释239c"));
        wr239.Quizzes.Add(new WordRootQuiz(wr239.Id, "word239c 的意思是什么？", new[] { "快速的", "意思239a", "意思239c", "意思239b" }, 2));
        roots.Add(wr239);

        // 240: root240
        var wr240 = new WordRoot(240, "root240", "Latin", "含义240", "meaning240", "root240 是Latin词根，表示含义240。");
        wr240.Examples.Add(new WordRootExample(wr240.Id, "word240a", "", "root240", "", "意思240a", "解释240a"));
        wr240.Examples.Add(new WordRootExample(wr240.Id, "word240b", "re", "root240", "", "意思240b", "解释240b"));
        wr240.Examples.Add(new WordRootExample(wr240.Id, "word240c", "", "root240", "tion", "意思240c", "解释240c"));
        wr240.Quizzes.Add(new WordRootQuiz(wr240.Id, "word240c 的意思是什么？", new[] { "快速的", "意思240b", "意思240c", "意思240a" }, 2));
        roots.Add(wr240);

        // 241: root241
        var wr241 = new WordRoot(241, "root241", "Latin", "含义241", "meaning241", "root241 是Latin词根，表示含义241。");
        wr241.Examples.Add(new WordRootExample(wr241.Id, "word241a", "", "root241", "", "意思241a", "解释241a"));
        wr241.Examples.Add(new WordRootExample(wr241.Id, "word241b", "re", "root241", "", "意思241b", "解释241b"));
        wr241.Examples.Add(new WordRootExample(wr241.Id, "word241c", "", "root241", "tion", "意思241c", "解释241c"));
        wr241.Quizzes.Add(new WordRootQuiz(wr241.Id, "word241b 的意思是什么？", new[] { "意思241a", "快速的", "意思241c", "意思241b" }, 3));
        roots.Add(wr241);

        // 242: root242
        var wr242 = new WordRoot(242, "root242", "Latin", "含义242", "meaning242", "root242 是Latin词根，表示含义242。");
        wr242.Examples.Add(new WordRootExample(wr242.Id, "word242a", "", "root242", "", "意思242a", "解释242a"));
        wr242.Examples.Add(new WordRootExample(wr242.Id, "word242b", "re", "root242", "", "意思242b", "解释242b"));
        wr242.Examples.Add(new WordRootExample(wr242.Id, "word242c", "", "root242", "tion", "意思242c", "解释242c"));
        wr242.Quizzes.Add(new WordRootQuiz(wr242.Id, "word242c 的意思是什么？", new[] { "意思242a", "快速的", "意思242b", "意思242c" }, 3));
        roots.Add(wr242);

        // 243: root243
        var wr243 = new WordRoot(243, "root243", "Latin", "含义243", "meaning243", "root243 是Latin词根，表示含义243。");
        wr243.Examples.Add(new WordRootExample(wr243.Id, "word243a", "", "root243", "", "意思243a", "解释243a"));
        wr243.Examples.Add(new WordRootExample(wr243.Id, "word243b", "re", "root243", "", "意思243b", "解释243b"));
        wr243.Examples.Add(new WordRootExample(wr243.Id, "word243c", "", "root243", "tion", "意思243c", "解释243c"));
        wr243.Quizzes.Add(new WordRootQuiz(wr243.Id, "word243a 的意思是什么？", new[] { "快速的", "意思243c", "意思243b", "意思243a" }, 3));
        roots.Add(wr243);

        // 244: root244
        var wr244 = new WordRoot(244, "root244", "Latin", "含义244", "meaning244", "root244 是Latin词根，表示含义244。");
        wr244.Examples.Add(new WordRootExample(wr244.Id, "word244a", "", "root244", "", "意思244a", "解释244a"));
        wr244.Examples.Add(new WordRootExample(wr244.Id, "word244b", "re", "root244", "", "意思244b", "解释244b"));
        wr244.Examples.Add(new WordRootExample(wr244.Id, "word244c", "", "root244", "tion", "意思244c", "解释244c"));
        wr244.Quizzes.Add(new WordRootQuiz(wr244.Id, "word244b 的意思是什么？", new[] { "意思244b", "意思244a", "快速的", "意思244c" }, 0));
        roots.Add(wr244);

        // 245: root245
        var wr245 = new WordRoot(245, "root245", "Latin", "含义245", "meaning245", "root245 是Latin词根，表示含义245。");
        wr245.Examples.Add(new WordRootExample(wr245.Id, "word245a", "", "root245", "", "意思245a", "解释245a"));
        wr245.Examples.Add(new WordRootExample(wr245.Id, "word245b", "re", "root245", "", "意思245b", "解释245b"));
        wr245.Examples.Add(new WordRootExample(wr245.Id, "word245c", "", "root245", "tion", "意思245c", "解释245c"));
        wr245.Quizzes.Add(new WordRootQuiz(wr245.Id, "word245a 的意思是什么？", new[] { "快速的", "意思245a", "意思245b", "意思245c" }, 1));
        roots.Add(wr245);

        // 246: root246
        var wr246 = new WordRoot(246, "root246", "Latin", "含义246", "meaning246", "root246 是Latin词根，表示含义246。");
        wr246.Examples.Add(new WordRootExample(wr246.Id, "word246a", "", "root246", "", "意思246a", "解释246a"));
        wr246.Examples.Add(new WordRootExample(wr246.Id, "word246b", "re", "root246", "", "意思246b", "解释246b"));
        wr246.Examples.Add(new WordRootExample(wr246.Id, "word246c", "", "root246", "tion", "意思246c", "解释246c"));
        wr246.Quizzes.Add(new WordRootQuiz(wr246.Id, "word246c 的意思是什么？", new[] { "意思246a", "意思246c", "意思246b", "快速的" }, 1));
        roots.Add(wr246);

        // 247: root247
        var wr247 = new WordRoot(247, "root247", "Latin", "含义247", "meaning247", "root247 是Latin词根，表示含义247。");
        wr247.Examples.Add(new WordRootExample(wr247.Id, "word247a", "", "root247", "", "意思247a", "解释247a"));
        wr247.Examples.Add(new WordRootExample(wr247.Id, "word247b", "re", "root247", "", "意思247b", "解释247b"));
        wr247.Examples.Add(new WordRootExample(wr247.Id, "word247c", "", "root247", "tion", "意思247c", "解释247c"));
        wr247.Quizzes.Add(new WordRootQuiz(wr247.Id, "word247c 的意思是什么？", new[] { "意思247a", "意思247c", "快速的", "意思247b" }, 1));
        roots.Add(wr247);

        // 248: root248
        var wr248 = new WordRoot(248, "root248", "Latin", "含义248", "meaning248", "root248 是Latin词根，表示含义248。");
        wr248.Examples.Add(new WordRootExample(wr248.Id, "word248a", "", "root248", "", "意思248a", "解释248a"));
        wr248.Examples.Add(new WordRootExample(wr248.Id, "word248b", "re", "root248", "", "意思248b", "解释248b"));
        wr248.Examples.Add(new WordRootExample(wr248.Id, "word248c", "", "root248", "tion", "意思248c", "解释248c"));
        wr248.Quizzes.Add(new WordRootQuiz(wr248.Id, "word248b 的意思是什么？", new[] { "快速的", "意思248a", "意思248c", "意思248b" }, 3));
        roots.Add(wr248);

        // 249: root249
        var wr249 = new WordRoot(249, "root249", "Latin", "含义249", "meaning249", "root249 是Latin词根，表示含义249。");
        wr249.Examples.Add(new WordRootExample(wr249.Id, "word249a", "", "root249", "", "意思249a", "解释249a"));
        wr249.Examples.Add(new WordRootExample(wr249.Id, "word249b", "re", "root249", "", "意思249b", "解释249b"));
        wr249.Examples.Add(new WordRootExample(wr249.Id, "word249c", "", "root249", "tion", "意思249c", "解释249c"));
        wr249.Quizzes.Add(new WordRootQuiz(wr249.Id, "word249b 的意思是什么？", new[] { "意思249b", "快速的", "意思249c", "意思249a" }, 0));
        roots.Add(wr249);

        // 250: root250
        var wr250 = new WordRoot(250, "root250", "Latin", "含义250", "meaning250", "root250 是Latin词根，表示含义250。");
        wr250.Examples.Add(new WordRootExample(wr250.Id, "word250a", "", "root250", "", "意思250a", "解释250a"));
        wr250.Examples.Add(new WordRootExample(wr250.Id, "word250b", "re", "root250", "", "意思250b", "解释250b"));
        wr250.Examples.Add(new WordRootExample(wr250.Id, "word250c", "", "root250", "tion", "意思250c", "解释250c"));
        wr250.Quizzes.Add(new WordRootQuiz(wr250.Id, "word250a 的意思是什么？", new[] { "意思250b", "意思250a", "意思250c", "快速的" }, 1));
        roots.Add(wr250);

        // 251: root251
        var wr251 = new WordRoot(251, "root251", "Latin", "含义251", "meaning251", "root251 是Latin词根，表示含义251。");
        wr251.Examples.Add(new WordRootExample(wr251.Id, "word251a", "", "root251", "", "意思251a", "解释251a"));
        wr251.Examples.Add(new WordRootExample(wr251.Id, "word251b", "re", "root251", "", "意思251b", "解释251b"));
        wr251.Examples.Add(new WordRootExample(wr251.Id, "word251c", "", "root251", "tion", "意思251c", "解释251c"));
        wr251.Quizzes.Add(new WordRootQuiz(wr251.Id, "word251b 的意思是什么？", new[] { "意思251b", "意思251a", "快速的", "意思251c" }, 0));
        roots.Add(wr251);

        // 252: root252
        var wr252 = new WordRoot(252, "root252", "Latin", "含义252", "meaning252", "root252 是Latin词根，表示含义252。");
        wr252.Examples.Add(new WordRootExample(wr252.Id, "word252a", "", "root252", "", "意思252a", "解释252a"));
        wr252.Examples.Add(new WordRootExample(wr252.Id, "word252b", "re", "root252", "", "意思252b", "解释252b"));
        wr252.Examples.Add(new WordRootExample(wr252.Id, "word252c", "", "root252", "tion", "意思252c", "解释252c"));
        wr252.Quizzes.Add(new WordRootQuiz(wr252.Id, "word252a 的意思是什么？", new[] { "意思252c", "意思252a", "意思252b", "快速的" }, 1));
        roots.Add(wr252);

        // 253: root253
        var wr253 = new WordRoot(253, "root253", "Latin", "含义253", "meaning253", "root253 是Latin词根，表示含义253。");
        wr253.Examples.Add(new WordRootExample(wr253.Id, "word253a", "", "root253", "", "意思253a", "解释253a"));
        wr253.Examples.Add(new WordRootExample(wr253.Id, "word253b", "re", "root253", "", "意思253b", "解释253b"));
        wr253.Examples.Add(new WordRootExample(wr253.Id, "word253c", "", "root253", "tion", "意思253c", "解释253c"));
        wr253.Quizzes.Add(new WordRootQuiz(wr253.Id, "word253a 的意思是什么？", new[] { "意思253b", "意思253c", "意思253a", "快速的" }, 2));
        roots.Add(wr253);

        // 254: root254
        var wr254 = new WordRoot(254, "root254", "Latin", "含义254", "meaning254", "root254 是Latin词根，表示含义254。");
        wr254.Examples.Add(new WordRootExample(wr254.Id, "word254a", "", "root254", "", "意思254a", "解释254a"));
        wr254.Examples.Add(new WordRootExample(wr254.Id, "word254b", "re", "root254", "", "意思254b", "解释254b"));
        wr254.Examples.Add(new WordRootExample(wr254.Id, "word254c", "", "root254", "tion", "意思254c", "解释254c"));
        wr254.Quizzes.Add(new WordRootQuiz(wr254.Id, "word254a 的意思是什么？", new[] { "意思254a", "意思254b", "意思254c", "快速的" }, 0));
        roots.Add(wr254);

        // 255: root255
        var wr255 = new WordRoot(255, "root255", "Latin", "含义255", "meaning255", "root255 是Latin词根，表示含义255。");
        wr255.Examples.Add(new WordRootExample(wr255.Id, "word255a", "", "root255", "", "意思255a", "解释255a"));
        wr255.Examples.Add(new WordRootExample(wr255.Id, "word255b", "re", "root255", "", "意思255b", "解释255b"));
        wr255.Examples.Add(new WordRootExample(wr255.Id, "word255c", "", "root255", "tion", "意思255c", "解释255c"));
        wr255.Quizzes.Add(new WordRootQuiz(wr255.Id, "word255a 的意思是什么？", new[] { "意思255c", "快速的", "意思255b", "意思255a" }, 3));
        roots.Add(wr255);

        // 256: root256
        var wr256 = new WordRoot(256, "root256", "Latin", "含义256", "meaning256", "root256 是Latin词根，表示含义256。");
        wr256.Examples.Add(new WordRootExample(wr256.Id, "word256a", "", "root256", "", "意思256a", "解释256a"));
        wr256.Examples.Add(new WordRootExample(wr256.Id, "word256b", "re", "root256", "", "意思256b", "解释256b"));
        wr256.Examples.Add(new WordRootExample(wr256.Id, "word256c", "", "root256", "tion", "意思256c", "解释256c"));
        wr256.Quizzes.Add(new WordRootQuiz(wr256.Id, "word256a 的意思是什么？", new[] { "意思256a", "快速的", "意思256b", "意思256c" }, 0));
        roots.Add(wr256);

        // 257: root257
        var wr257 = new WordRoot(257, "root257", "Latin", "含义257", "meaning257", "root257 是Latin词根，表示含义257。");
        wr257.Examples.Add(new WordRootExample(wr257.Id, "word257a", "", "root257", "", "意思257a", "解释257a"));
        wr257.Examples.Add(new WordRootExample(wr257.Id, "word257b", "re", "root257", "", "意思257b", "解释257b"));
        wr257.Examples.Add(new WordRootExample(wr257.Id, "word257c", "", "root257", "tion", "意思257c", "解释257c"));
        wr257.Quizzes.Add(new WordRootQuiz(wr257.Id, "word257b 的意思是什么？", new[] { "意思257c", "意思257a", "快速的", "意思257b" }, 3));
        roots.Add(wr257);

        // 258: root258
        var wr258 = new WordRoot(258, "root258", "Latin", "含义258", "meaning258", "root258 是Latin词根，表示含义258。");
        wr258.Examples.Add(new WordRootExample(wr258.Id, "word258a", "", "root258", "", "意思258a", "解释258a"));
        wr258.Examples.Add(new WordRootExample(wr258.Id, "word258b", "re", "root258", "", "意思258b", "解释258b"));
        wr258.Examples.Add(new WordRootExample(wr258.Id, "word258c", "", "root258", "tion", "意思258c", "解释258c"));
        wr258.Quizzes.Add(new WordRootQuiz(wr258.Id, "word258c 的意思是什么？", new[] { "意思258c", "意思258a", "快速的", "意思258b" }, 0));
        roots.Add(wr258);

        // 259: root259
        var wr259 = new WordRoot(259, "root259", "Latin", "含义259", "meaning259", "root259 是Latin词根，表示含义259。");
        wr259.Examples.Add(new WordRootExample(wr259.Id, "word259a", "", "root259", "", "意思259a", "解释259a"));
        wr259.Examples.Add(new WordRootExample(wr259.Id, "word259b", "re", "root259", "", "意思259b", "解释259b"));
        wr259.Examples.Add(new WordRootExample(wr259.Id, "word259c", "", "root259", "tion", "意思259c", "解释259c"));
        wr259.Quizzes.Add(new WordRootQuiz(wr259.Id, "word259b 的意思是什么？", new[] { "意思259b", "意思259a", "快速的", "意思259c" }, 0));
        roots.Add(wr259);

        // 260: root260
        var wr260 = new WordRoot(260, "root260", "Latin", "含义260", "meaning260", "root260 是Latin词根，表示含义260。");
        wr260.Examples.Add(new WordRootExample(wr260.Id, "word260a", "", "root260", "", "意思260a", "解释260a"));
        wr260.Examples.Add(new WordRootExample(wr260.Id, "word260b", "re", "root260", "", "意思260b", "解释260b"));
        wr260.Examples.Add(new WordRootExample(wr260.Id, "word260c", "", "root260", "tion", "意思260c", "解释260c"));
        wr260.Quizzes.Add(new WordRootQuiz(wr260.Id, "word260a 的意思是什么？", new[] { "意思260a", "意思260c", "意思260b", "快速的" }, 0));
        roots.Add(wr260);

        // 261: root261
        var wr261 = new WordRoot(261, "root261", "Latin", "含义261", "meaning261", "root261 是Latin词根，表示含义261。");
        wr261.Examples.Add(new WordRootExample(wr261.Id, "word261a", "", "root261", "", "意思261a", "解释261a"));
        wr261.Examples.Add(new WordRootExample(wr261.Id, "word261b", "re", "root261", "", "意思261b", "解释261b"));
        wr261.Examples.Add(new WordRootExample(wr261.Id, "word261c", "", "root261", "tion", "意思261c", "解释261c"));
        wr261.Quizzes.Add(new WordRootQuiz(wr261.Id, "word261c 的意思是什么？", new[] { "意思261a", "意思261b", "意思261c", "快速的" }, 2));
        roots.Add(wr261);

        // 262: root262
        var wr262 = new WordRoot(262, "root262", "Latin", "含义262", "meaning262", "root262 是Latin词根，表示含义262。");
        wr262.Examples.Add(new WordRootExample(wr262.Id, "word262a", "", "root262", "", "意思262a", "解释262a"));
        wr262.Examples.Add(new WordRootExample(wr262.Id, "word262b", "re", "root262", "", "意思262b", "解释262b"));
        wr262.Examples.Add(new WordRootExample(wr262.Id, "word262c", "", "root262", "tion", "意思262c", "解释262c"));
        wr262.Quizzes.Add(new WordRootQuiz(wr262.Id, "word262c 的意思是什么？", new[] { "意思262c", "意思262a", "意思262b", "快速的" }, 0));
        roots.Add(wr262);

        // 263: root263
        var wr263 = new WordRoot(263, "root263", "Latin", "含义263", "meaning263", "root263 是Latin词根，表示含义263。");
        wr263.Examples.Add(new WordRootExample(wr263.Id, "word263a", "", "root263", "", "意思263a", "解释263a"));
        wr263.Examples.Add(new WordRootExample(wr263.Id, "word263b", "re", "root263", "", "意思263b", "解释263b"));
        wr263.Examples.Add(new WordRootExample(wr263.Id, "word263c", "", "root263", "tion", "意思263c", "解释263c"));
        wr263.Quizzes.Add(new WordRootQuiz(wr263.Id, "word263b 的意思是什么？", new[] { "意思263a", "快速的", "意思263c", "意思263b" }, 3));
        roots.Add(wr263);

        // 264: root264
        var wr264 = new WordRoot(264, "root264", "Latin", "含义264", "meaning264", "root264 是Latin词根，表示含义264。");
        wr264.Examples.Add(new WordRootExample(wr264.Id, "word264a", "", "root264", "", "意思264a", "解释264a"));
        wr264.Examples.Add(new WordRootExample(wr264.Id, "word264b", "re", "root264", "", "意思264b", "解释264b"));
        wr264.Examples.Add(new WordRootExample(wr264.Id, "word264c", "", "root264", "tion", "意思264c", "解释264c"));
        wr264.Quizzes.Add(new WordRootQuiz(wr264.Id, "word264c 的意思是什么？", new[] { "意思264b", "意思264a", "意思264c", "快速的" }, 2));
        roots.Add(wr264);

        // 265: root265
        var wr265 = new WordRoot(265, "root265", "Latin", "含义265", "meaning265", "root265 是Latin词根，表示含义265。");
        wr265.Examples.Add(new WordRootExample(wr265.Id, "word265a", "", "root265", "", "意思265a", "解释265a"));
        wr265.Examples.Add(new WordRootExample(wr265.Id, "word265b", "re", "root265", "", "意思265b", "解释265b"));
        wr265.Examples.Add(new WordRootExample(wr265.Id, "word265c", "", "root265", "tion", "意思265c", "解释265c"));
        wr265.Quizzes.Add(new WordRootQuiz(wr265.Id, "word265c 的意思是什么？", new[] { "快速的", "意思265b", "意思265c", "意思265a" }, 2));
        roots.Add(wr265);

        // 266: root266
        var wr266 = new WordRoot(266, "root266", "Latin", "含义266", "meaning266", "root266 是Latin词根，表示含义266。");
        wr266.Examples.Add(new WordRootExample(wr266.Id, "word266a", "", "root266", "", "意思266a", "解释266a"));
        wr266.Examples.Add(new WordRootExample(wr266.Id, "word266b", "re", "root266", "", "意思266b", "解释266b"));
        wr266.Examples.Add(new WordRootExample(wr266.Id, "word266c", "", "root266", "tion", "意思266c", "解释266c"));
        wr266.Quizzes.Add(new WordRootQuiz(wr266.Id, "word266c 的意思是什么？", new[] { "意思266a", "意思266c", "快速的", "意思266b" }, 1));
        roots.Add(wr266);

        // 267: root267
        var wr267 = new WordRoot(267, "root267", "Latin", "含义267", "meaning267", "root267 是Latin词根，表示含义267。");
        wr267.Examples.Add(new WordRootExample(wr267.Id, "word267a", "", "root267", "", "意思267a", "解释267a"));
        wr267.Examples.Add(new WordRootExample(wr267.Id, "word267b", "re", "root267", "", "意思267b", "解释267b"));
        wr267.Examples.Add(new WordRootExample(wr267.Id, "word267c", "", "root267", "tion", "意思267c", "解释267c"));
        wr267.Quizzes.Add(new WordRootQuiz(wr267.Id, "word267b 的意思是什么？", new[] { "意思267b", "快速的", "意思267a", "意思267c" }, 0));
        roots.Add(wr267);

        // 268: root268
        var wr268 = new WordRoot(268, "root268", "Latin", "含义268", "meaning268", "root268 是Latin词根，表示含义268。");
        wr268.Examples.Add(new WordRootExample(wr268.Id, "word268a", "", "root268", "", "意思268a", "解释268a"));
        wr268.Examples.Add(new WordRootExample(wr268.Id, "word268b", "re", "root268", "", "意思268b", "解释268b"));
        wr268.Examples.Add(new WordRootExample(wr268.Id, "word268c", "", "root268", "tion", "意思268c", "解释268c"));
        wr268.Quizzes.Add(new WordRootQuiz(wr268.Id, "word268a 的意思是什么？", new[] { "意思268c", "意思268a", "快速的", "意思268b" }, 1));
        roots.Add(wr268);

        // 269: root269
        var wr269 = new WordRoot(269, "root269", "Latin", "含义269", "meaning269", "root269 是Latin词根，表示含义269。");
        wr269.Examples.Add(new WordRootExample(wr269.Id, "word269a", "", "root269", "", "意思269a", "解释269a"));
        wr269.Examples.Add(new WordRootExample(wr269.Id, "word269b", "re", "root269", "", "意思269b", "解释269b"));
        wr269.Examples.Add(new WordRootExample(wr269.Id, "word269c", "", "root269", "tion", "意思269c", "解释269c"));
        wr269.Quizzes.Add(new WordRootQuiz(wr269.Id, "word269a 的意思是什么？", new[] { "意思269a", "意思269c", "快速的", "意思269b" }, 0));
        roots.Add(wr269);

        // 270: root270
        var wr270 = new WordRoot(270, "root270", "Latin", "含义270", "meaning270", "root270 是Latin词根，表示含义270。");
        wr270.Examples.Add(new WordRootExample(wr270.Id, "word270a", "", "root270", "", "意思270a", "解释270a"));
        wr270.Examples.Add(new WordRootExample(wr270.Id, "word270b", "re", "root270", "", "意思270b", "解释270b"));
        wr270.Examples.Add(new WordRootExample(wr270.Id, "word270c", "", "root270", "tion", "意思270c", "解释270c"));
        wr270.Quizzes.Add(new WordRootQuiz(wr270.Id, "word270b 的意思是什么？", new[] { "意思270c", "意思270b", "意思270a", "快速的" }, 1));
        roots.Add(wr270);

        // 271: root271
        var wr271 = new WordRoot(271, "root271", "Latin", "含义271", "meaning271", "root271 是Latin词根，表示含义271。");
        wr271.Examples.Add(new WordRootExample(wr271.Id, "word271a", "", "root271", "", "意思271a", "解释271a"));
        wr271.Examples.Add(new WordRootExample(wr271.Id, "word271b", "re", "root271", "", "意思271b", "解释271b"));
        wr271.Examples.Add(new WordRootExample(wr271.Id, "word271c", "", "root271", "tion", "意思271c", "解释271c"));
        wr271.Quizzes.Add(new WordRootQuiz(wr271.Id, "word271b 的意思是什么？", new[] { "意思271c", "意思271a", "意思271b", "快速的" }, 2));
        roots.Add(wr271);

        // 272: root272
        var wr272 = new WordRoot(272, "root272", "Latin", "含义272", "meaning272", "root272 是Latin词根，表示含义272。");
        wr272.Examples.Add(new WordRootExample(wr272.Id, "word272a", "", "root272", "", "意思272a", "解释272a"));
        wr272.Examples.Add(new WordRootExample(wr272.Id, "word272b", "re", "root272", "", "意思272b", "解释272b"));
        wr272.Examples.Add(new WordRootExample(wr272.Id, "word272c", "", "root272", "tion", "意思272c", "解释272c"));
        wr272.Quizzes.Add(new WordRootQuiz(wr272.Id, "word272c 的意思是什么？", new[] { "意思272a", "意思272c", "快速的", "意思272b" }, 1));
        roots.Add(wr272);

        // 273: root273
        var wr273 = new WordRoot(273, "root273", "Latin", "含义273", "meaning273", "root273 是Latin词根，表示含义273。");
        wr273.Examples.Add(new WordRootExample(wr273.Id, "word273a", "", "root273", "", "意思273a", "解释273a"));
        wr273.Examples.Add(new WordRootExample(wr273.Id, "word273b", "re", "root273", "", "意思273b", "解释273b"));
        wr273.Examples.Add(new WordRootExample(wr273.Id, "word273c", "", "root273", "tion", "意思273c", "解释273c"));
        wr273.Quizzes.Add(new WordRootQuiz(wr273.Id, "word273c 的意思是什么？", new[] { "快速的", "意思273b", "意思273c", "意思273a" }, 2));
        roots.Add(wr273);

        // 274: root274
        var wr274 = new WordRoot(274, "root274", "Latin", "含义274", "meaning274", "root274 是Latin词根，表示含义274。");
        wr274.Examples.Add(new WordRootExample(wr274.Id, "word274a", "", "root274", "", "意思274a", "解释274a"));
        wr274.Examples.Add(new WordRootExample(wr274.Id, "word274b", "re", "root274", "", "意思274b", "解释274b"));
        wr274.Examples.Add(new WordRootExample(wr274.Id, "word274c", "", "root274", "tion", "意思274c", "解释274c"));
        wr274.Quizzes.Add(new WordRootQuiz(wr274.Id, "word274a 的意思是什么？", new[] { "意思274b", "意思274a", "快速的", "意思274c" }, 1));
        roots.Add(wr274);

        // 275: root275
        var wr275 = new WordRoot(275, "root275", "Latin", "含义275", "meaning275", "root275 是Latin词根，表示含义275。");
        wr275.Examples.Add(new WordRootExample(wr275.Id, "word275a", "", "root275", "", "意思275a", "解释275a"));
        wr275.Examples.Add(new WordRootExample(wr275.Id, "word275b", "re", "root275", "", "意思275b", "解释275b"));
        wr275.Examples.Add(new WordRootExample(wr275.Id, "word275c", "", "root275", "tion", "意思275c", "解释275c"));
        wr275.Quizzes.Add(new WordRootQuiz(wr275.Id, "word275b 的意思是什么？", new[] { "意思275a", "快速的", "意思275b", "意思275c" }, 2));
        roots.Add(wr275);

        // 276: root276
        var wr276 = new WordRoot(276, "root276", "Latin", "含义276", "meaning276", "root276 是Latin词根，表示含义276。");
        wr276.Examples.Add(new WordRootExample(wr276.Id, "word276a", "", "root276", "", "意思276a", "解释276a"));
        wr276.Examples.Add(new WordRootExample(wr276.Id, "word276b", "re", "root276", "", "意思276b", "解释276b"));
        wr276.Examples.Add(new WordRootExample(wr276.Id, "word276c", "", "root276", "tion", "意思276c", "解释276c"));
        wr276.Quizzes.Add(new WordRootQuiz(wr276.Id, "word276a 的意思是什么？", new[] { "快速的", "意思276c", "意思276b", "意思276a" }, 3));
        roots.Add(wr276);

        // 277: root277
        var wr277 = new WordRoot(277, "root277", "Latin", "含义277", "meaning277", "root277 是Latin词根，表示含义277。");
        wr277.Examples.Add(new WordRootExample(wr277.Id, "word277a", "", "root277", "", "意思277a", "解释277a"));
        wr277.Examples.Add(new WordRootExample(wr277.Id, "word277b", "re", "root277", "", "意思277b", "解释277b"));
        wr277.Examples.Add(new WordRootExample(wr277.Id, "word277c", "", "root277", "tion", "意思277c", "解释277c"));
        wr277.Quizzes.Add(new WordRootQuiz(wr277.Id, "word277b 的意思是什么？", new[] { "意思277c", "意思277b", "意思277a", "快速的" }, 1));
        roots.Add(wr277);

        // 278: root278
        var wr278 = new WordRoot(278, "root278", "Latin", "含义278", "meaning278", "root278 是Latin词根，表示含义278。");
        wr278.Examples.Add(new WordRootExample(wr278.Id, "word278a", "", "root278", "", "意思278a", "解释278a"));
        wr278.Examples.Add(new WordRootExample(wr278.Id, "word278b", "re", "root278", "", "意思278b", "解释278b"));
        wr278.Examples.Add(new WordRootExample(wr278.Id, "word278c", "", "root278", "tion", "意思278c", "解释278c"));
        wr278.Quizzes.Add(new WordRootQuiz(wr278.Id, "word278b 的意思是什么？", new[] { "意思278b", "意思278a", "快速的", "意思278c" }, 0));
        roots.Add(wr278);

        // 279: root279
        var wr279 = new WordRoot(279, "root279", "Latin", "含义279", "meaning279", "root279 是Latin词根，表示含义279。");
        wr279.Examples.Add(new WordRootExample(wr279.Id, "word279a", "", "root279", "", "意思279a", "解释279a"));
        wr279.Examples.Add(new WordRootExample(wr279.Id, "word279b", "re", "root279", "", "意思279b", "解释279b"));
        wr279.Examples.Add(new WordRootExample(wr279.Id, "word279c", "", "root279", "tion", "意思279c", "解释279c"));
        wr279.Quizzes.Add(new WordRootQuiz(wr279.Id, "word279b 的意思是什么？", new[] { "意思279a", "意思279b", "意思279c", "快速的" }, 1));
        roots.Add(wr279);

        // 280: root280
        var wr280 = new WordRoot(280, "root280", "Latin", "含义280", "meaning280", "root280 是Latin词根，表示含义280。");
        wr280.Examples.Add(new WordRootExample(wr280.Id, "word280a", "", "root280", "", "意思280a", "解释280a"));
        wr280.Examples.Add(new WordRootExample(wr280.Id, "word280b", "re", "root280", "", "意思280b", "解释280b"));
        wr280.Examples.Add(new WordRootExample(wr280.Id, "word280c", "", "root280", "tion", "意思280c", "解释280c"));
        wr280.Quizzes.Add(new WordRootQuiz(wr280.Id, "word280a 的意思是什么？", new[] { "意思280b", "意思280c", "意思280a", "快速的" }, 2));
        roots.Add(wr280);

        // 281: suffix281
        var wr281 = new WordRoot(281, "suffix281", "Latin", "后缀281", "suffix281", "suffix281 是Latin词根，表示后缀281。");
        wr281.Examples.Add(new WordRootExample(wr281.Id, "example281a", "", "base", "suffix281", "例词281a", "说明281a"));
        wr281.Examples.Add(new WordRootExample(wr281.Id, "example281b", "pre", "base", "suffix281", "例词281b", "说明281b"));
        wr281.Quizzes.Add(new WordRootQuiz(wr281.Id, "example281b 的意思是什么？", new[] { "例词281b", "例词281a", "美丽的", "快速的" }, 0));
        roots.Add(wr281);

        // 282: suffix282
        var wr282 = new WordRoot(282, "suffix282", "Latin", "后缀282", "suffix282", "suffix282 是Latin词根，表示后缀282。");
        wr282.Examples.Add(new WordRootExample(wr282.Id, "example282a", "", "base", "suffix282", "例词282a", "说明282a"));
        wr282.Examples.Add(new WordRootExample(wr282.Id, "example282b", "pre", "base", "suffix282", "例词282b", "说明282b"));
        wr282.Quizzes.Add(new WordRootQuiz(wr282.Id, "example282b 的意思是什么？", new[] { "例词282a", "例词282b", "快速的", "美丽的" }, 1));
        roots.Add(wr282);

        // 283: suffix283
        var wr283 = new WordRoot(283, "suffix283", "Latin", "后缀283", "suffix283", "suffix283 是Latin词根，表示后缀283。");
        wr283.Examples.Add(new WordRootExample(wr283.Id, "example283a", "", "base", "suffix283", "例词283a", "说明283a"));
        wr283.Examples.Add(new WordRootExample(wr283.Id, "example283b", "pre", "base", "suffix283", "例词283b", "说明283b"));
        wr283.Quizzes.Add(new WordRootQuiz(wr283.Id, "example283a 的意思是什么？", new[] { "美丽的", "例词283a", "快速的", "例词283b" }, 1));
        roots.Add(wr283);

        // 284: suffix284
        var wr284 = new WordRoot(284, "suffix284", "Latin", "后缀284", "suffix284", "suffix284 是Latin词根，表示后缀284。");
        wr284.Examples.Add(new WordRootExample(wr284.Id, "example284a", "", "base", "suffix284", "例词284a", "说明284a"));
        wr284.Examples.Add(new WordRootExample(wr284.Id, "example284b", "pre", "base", "suffix284", "例词284b", "说明284b"));
        wr284.Quizzes.Add(new WordRootQuiz(wr284.Id, "example284a 的意思是什么？", new[] { "快速的", "美丽的", "例词284a", "例词284b" }, 2));
        roots.Add(wr284);

        // 285: suffix285
        var wr285 = new WordRoot(285, "suffix285", "Latin", "后缀285", "suffix285", "suffix285 是Latin词根，表示后缀285。");
        wr285.Examples.Add(new WordRootExample(wr285.Id, "example285a", "", "base", "suffix285", "例词285a", "说明285a"));
        wr285.Examples.Add(new WordRootExample(wr285.Id, "example285b", "pre", "base", "suffix285", "例词285b", "说明285b"));
        wr285.Quizzes.Add(new WordRootQuiz(wr285.Id, "example285b 的意思是什么？", new[] { "美丽的", "例词285b", "例词285a", "快速的" }, 1));
        roots.Add(wr285);

        // 286: suffix286
        var wr286 = new WordRoot(286, "suffix286", "Latin", "后缀286", "suffix286", "suffix286 是Latin词根，表示后缀286。");
        wr286.Examples.Add(new WordRootExample(wr286.Id, "example286a", "", "base", "suffix286", "例词286a", "说明286a"));
        wr286.Examples.Add(new WordRootExample(wr286.Id, "example286b", "pre", "base", "suffix286", "例词286b", "说明286b"));
        wr286.Quizzes.Add(new WordRootQuiz(wr286.Id, "example286b 的意思是什么？", new[] { "例词286a", "快速的", "美丽的", "例词286b" }, 3));
        roots.Add(wr286);

        // 287: suffix287
        var wr287 = new WordRoot(287, "suffix287", "Latin", "后缀287", "suffix287", "suffix287 是Latin词根，表示后缀287。");
        wr287.Examples.Add(new WordRootExample(wr287.Id, "example287a", "", "base", "suffix287", "例词287a", "说明287a"));
        wr287.Examples.Add(new WordRootExample(wr287.Id, "example287b", "pre", "base", "suffix287", "例词287b", "说明287b"));
        wr287.Quizzes.Add(new WordRootQuiz(wr287.Id, "example287a 的意思是什么？", new[] { "例词287b", "例词287a", "美丽的", "快速的" }, 1));
        roots.Add(wr287);

        // 288: suffix288
        var wr288 = new WordRoot(288, "suffix288", "Latin", "后缀288", "suffix288", "suffix288 是Latin词根，表示后缀288。");
        wr288.Examples.Add(new WordRootExample(wr288.Id, "example288a", "", "base", "suffix288", "例词288a", "说明288a"));
        wr288.Examples.Add(new WordRootExample(wr288.Id, "example288b", "pre", "base", "suffix288", "例词288b", "说明288b"));
        wr288.Quizzes.Add(new WordRootQuiz(wr288.Id, "example288a 的意思是什么？", new[] { "例词288a", "快速的", "例词288b", "美丽的" }, 0));
        roots.Add(wr288);

        // 289: suffix289
        var wr289 = new WordRoot(289, "suffix289", "Latin", "后缀289", "suffix289", "suffix289 是Latin词根，表示后缀289。");
        wr289.Examples.Add(new WordRootExample(wr289.Id, "example289a", "", "base", "suffix289", "例词289a", "说明289a"));
        wr289.Examples.Add(new WordRootExample(wr289.Id, "example289b", "pre", "base", "suffix289", "例词289b", "说明289b"));
        wr289.Quizzes.Add(new WordRootQuiz(wr289.Id, "example289a 的意思是什么？", new[] { "例词289a", "快速的", "例词289b", "美丽的" }, 0));
        roots.Add(wr289);

        // 290: suffix290
        var wr290 = new WordRoot(290, "suffix290", "Latin", "后缀290", "suffix290", "suffix290 是Latin词根，表示后缀290。");
        wr290.Examples.Add(new WordRootExample(wr290.Id, "example290a", "", "base", "suffix290", "例词290a", "说明290a"));
        wr290.Examples.Add(new WordRootExample(wr290.Id, "example290b", "pre", "base", "suffix290", "例词290b", "说明290b"));
        wr290.Quizzes.Add(new WordRootQuiz(wr290.Id, "example290a 的意思是什么？", new[] { "美丽的", "快速的", "例词290a", "例词290b" }, 2));
        roots.Add(wr290);

        // 291: suffix291
        var wr291 = new WordRoot(291, "suffix291", "Latin", "后缀291", "suffix291", "suffix291 是Latin词根，表示后缀291。");
        wr291.Examples.Add(new WordRootExample(wr291.Id, "example291a", "", "base", "suffix291", "例词291a", "说明291a"));
        wr291.Examples.Add(new WordRootExample(wr291.Id, "example291b", "pre", "base", "suffix291", "例词291b", "说明291b"));
        wr291.Quizzes.Add(new WordRootQuiz(wr291.Id, "example291b 的意思是什么？", new[] { "例词291b", "例词291a", "美丽的", "快速的" }, 0));
        roots.Add(wr291);

        // 292: suffix292
        var wr292 = new WordRoot(292, "suffix292", "Latin", "后缀292", "suffix292", "suffix292 是Latin词根，表示后缀292。");
        wr292.Examples.Add(new WordRootExample(wr292.Id, "example292a", "", "base", "suffix292", "例词292a", "说明292a"));
        wr292.Examples.Add(new WordRootExample(wr292.Id, "example292b", "pre", "base", "suffix292", "例词292b", "说明292b"));
        wr292.Quizzes.Add(new WordRootQuiz(wr292.Id, "example292a 的意思是什么？", new[] { "美丽的", "例词292b", "快速的", "例词292a" }, 3));
        roots.Add(wr292);

        // 293: suffix293
        var wr293 = new WordRoot(293, "suffix293", "Latin", "后缀293", "suffix293", "suffix293 是Latin词根，表示后缀293。");
        wr293.Examples.Add(new WordRootExample(wr293.Id, "example293a", "", "base", "suffix293", "例词293a", "说明293a"));
        wr293.Examples.Add(new WordRootExample(wr293.Id, "example293b", "pre", "base", "suffix293", "例词293b", "说明293b"));
        wr293.Quizzes.Add(new WordRootQuiz(wr293.Id, "example293b 的意思是什么？", new[] { "快速的", "例词293b", "例词293a", "美丽的" }, 1));
        roots.Add(wr293);

        // 294: suffix294
        var wr294 = new WordRoot(294, "suffix294", "Latin", "后缀294", "suffix294", "suffix294 是Latin词根，表示后缀294。");
        wr294.Examples.Add(new WordRootExample(wr294.Id, "example294a", "", "base", "suffix294", "例词294a", "说明294a"));
        wr294.Examples.Add(new WordRootExample(wr294.Id, "example294b", "pre", "base", "suffix294", "例词294b", "说明294b"));
        wr294.Quizzes.Add(new WordRootQuiz(wr294.Id, "example294a 的意思是什么？", new[] { "快速的", "美丽的", "例词294a", "例词294b" }, 2));
        roots.Add(wr294);

        // 295: suffix295
        var wr295 = new WordRoot(295, "suffix295", "Latin", "后缀295", "suffix295", "suffix295 是Latin词根，表示后缀295。");
        wr295.Examples.Add(new WordRootExample(wr295.Id, "example295a", "", "base", "suffix295", "例词295a", "说明295a"));
        wr295.Examples.Add(new WordRootExample(wr295.Id, "example295b", "pre", "base", "suffix295", "例词295b", "说明295b"));
        wr295.Quizzes.Add(new WordRootQuiz(wr295.Id, "example295b 的意思是什么？", new[] { "例词295b", "美丽的", "例词295a", "快速的" }, 0));
        roots.Add(wr295);

        // 296: suffix296
        var wr296 = new WordRoot(296, "suffix296", "Latin", "后缀296", "suffix296", "suffix296 是Latin词根，表示后缀296。");
        wr296.Examples.Add(new WordRootExample(wr296.Id, "example296a", "", "base", "suffix296", "例词296a", "说明296a"));
        wr296.Examples.Add(new WordRootExample(wr296.Id, "example296b", "pre", "base", "suffix296", "例词296b", "说明296b"));
        wr296.Quizzes.Add(new WordRootQuiz(wr296.Id, "example296b 的意思是什么？", new[] { "美丽的", "快速的", "例词296b", "例词296a" }, 2));
        roots.Add(wr296);

        // 297: suffix297
        var wr297 = new WordRoot(297, "suffix297", "Latin", "后缀297", "suffix297", "suffix297 是Latin词根，表示后缀297。");
        wr297.Examples.Add(new WordRootExample(wr297.Id, "example297a", "", "base", "suffix297", "例词297a", "说明297a"));
        wr297.Examples.Add(new WordRootExample(wr297.Id, "example297b", "pre", "base", "suffix297", "例词297b", "说明297b"));
        wr297.Quizzes.Add(new WordRootQuiz(wr297.Id, "example297a 的意思是什么？", new[] { "例词297b", "快速的", "美丽的", "例词297a" }, 3));
        roots.Add(wr297);

        // 298: suffix298
        var wr298 = new WordRoot(298, "suffix298", "Latin", "后缀298", "suffix298", "suffix298 是Latin词根，表示后缀298。");
        wr298.Examples.Add(new WordRootExample(wr298.Id, "example298a", "", "base", "suffix298", "例词298a", "说明298a"));
        wr298.Examples.Add(new WordRootExample(wr298.Id, "example298b", "pre", "base", "suffix298", "例词298b", "说明298b"));
        wr298.Quizzes.Add(new WordRootQuiz(wr298.Id, "example298a 的意思是什么？", new[] { "快速的", "美丽的", "例词298a", "例词298b" }, 2));
        roots.Add(wr298);

        // 299: suffix299
        var wr299 = new WordRoot(299, "suffix299", "Latin", "后缀299", "suffix299", "suffix299 是Latin词根，表示后缀299。");
        wr299.Examples.Add(new WordRootExample(wr299.Id, "example299a", "", "base", "suffix299", "例词299a", "说明299a"));
        wr299.Examples.Add(new WordRootExample(wr299.Id, "example299b", "pre", "base", "suffix299", "例词299b", "说明299b"));
        wr299.Quizzes.Add(new WordRootQuiz(wr299.Id, "example299b 的意思是什么？", new[] { "美丽的", "快速的", "例词299a", "例词299b" }, 3));
        roots.Add(wr299);

        // 300: suffix300
        var wr300 = new WordRoot(300, "suffix300", "Latin", "后缀300", "suffix300", "suffix300 是Latin词根，表示后缀300。");
        wr300.Examples.Add(new WordRootExample(wr300.Id, "example300a", "", "base", "suffix300", "例词300a", "说明300a"));
        wr300.Examples.Add(new WordRootExample(wr300.Id, "example300b", "pre", "base", "suffix300", "例词300b", "说明300b"));
        wr300.Quizzes.Add(new WordRootQuiz(wr300.Id, "example300b 的意思是什么？", new[] { "例词300a", "美丽的", "例词300b", "快速的" }, 2));
        roots.Add(wr300);

        return roots.ToArray();
    }
}
