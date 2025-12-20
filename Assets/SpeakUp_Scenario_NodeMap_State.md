# SpeakUp! – Bản đồ kịch bản V2 (đa tuyến, nhiều hậu quả)

Tất cả nodes được viết dạng:
**NodeID • Mô tả • Lựa chọn → NextNode (TensionImpact, StateEffects)**

---

## GLOBAL STATE & RULES

**Biến cốt lõi:**

```csharp
int tension;                     // 0..100 – mức căng thẳng tại chỗ
const int SOFT_THRESHOLD = 70;
const int HARD_THRESHOLD = 85;

bool hasRecording;               // đã quay được clip chưa
bool recordingDiscovered;        // Hùng phát hiện bạn quay chưa
bool calledHelp;                 // đã gọi người lớn/bảo vệ chưa
bool teacherOnTheWay;            // giáo viên/giám thị đang tới
bool teacherArrived;             // giáo viên/giám thị đã tới

bool victimSafe;                 // Nam đã ra khỏi vùng nguy hiểm chưa
bool playerWalkedAway;           // người chơi đã bỏ đi hẳn chưa

int bystanderSupport;            // 0 = không ai, 1 = Linh, 2 = thêm người ngoài cuộc
int deEscalationSkill;           // 1..5 – kỹ năng hạ nhiệt

float victimTrust;               // -1..1 – Nam tin người chơi tới mức nào
float teacherTrust;              // -1..1 – giáo viên tin người chơi tới mức nào
float playerReputation;          // -1..1 – hình ảnh người chơi trong trường

float hungReputation;            // -1..1 – danh tiếng của Hùng (bị ghét/bị tẩy chay?)
float rumorLevel;                // 0..1 – mức độ tin đồn lan ra
bool socialMediaPosted;          // đã đăng clip lên mạng chưa
bool futureThreat;               // Hùng có “ghim” bạn sau vụ này không

bool aggressorFocusOnPlayer;     // Hùng đang tập trung vào người chơi
string currentNodeId;
string currentEndingId;
Khởi tạo gợi ý:

csharp
Sao chép mã
tension = 45;
hasRecording = false;
recordingDiscovered = false;
calledHelp = false;
teacherOnTheWay = false;
teacherArrived = false;
victimSafe = false;
playerWalkedAway = false;

bystanderSupport = 0;
deEscalationSkill = 3;

victimTrust = 0f;
teacherTrust = 0f;
playerReputation = 0f;
hungReputation = 0f;
rumorLevel = 0f;
socialMediaPosted = false;
futureThreat = false;

aggressorFocusOnPlayer = false;
currentNodeId = "S0";
currentEndingId = "";
Sau mỗi lựa chọn:

csharp
Sao chép mã
tension = clamp(tension + tensionDelta, 0, 100);
if (tension >= HARD_THRESHOLD) {
    currentNodeId = "S17"; // bạo lực
}
S0 – Bắt gặp
Mô tả: Bạn vừa hết tiết, đi trên hành lang vắng, thấy Hùng dí Nam vào tường, nói lớn: “Mày tưởng tao không biết mày méc chuyện hôm bữa hả?”.

Lựa chọn:

Giả vờ đi ngang, lắng nghe cho rõ đã → S1
(tension: +0, state: không đổi)

Hơi chạy lại nhanh, gọi “Ê Hùng, gì vậy?” → S2
(tension: +5, state: playerReputation += 0.1)

Rút điện thoại ra, mở camera nhưng để thấp, không ai để ý → S3
(tension: 0, state: hasRecording = true)

Quay đầu đi chỗ khác, giả vờ không thấy → E0
(tension: -5, state: playerWalkedAway = true; playerReputation -= 0.2; victimTrust -= 0.3)

S1 – Đứng nghe lén
Mô tả: Bạn đứng khuất sau cột, nghe Hùng vừa chửi vừa đập tay vào tường cạnh đầu Nam.

Lựa chọn:

Đợi thêm xem mức độ căng thẳng tới đâu → S3
(tension: +5, state: rumourLevel += 0.1)

Nhắn tin nhanh cho Linh: “Hành lang A, ra đây gấp” → S4
(tension: 0, state: bystanderSupport = max(1, bystanderSupport))

Thở dài, nghĩ “không nên xen vào chuyện người khác” rồi bỏ đi → E0
(tension: -5, state: playerWalkedAway = true; playerReputation -= 0.2; victimTrust -= 0.4)

S2 – Gọi trực diện
Mô tả: Bạn lên tiếng, Hùng quay sang, ánh mắt khó chịu, Nam liếc nhìn bạn như cầu cứu.

Lựa chọn:

Cười cười: “Ê bớt bớt, ở đây đông người mà” → S5
(tension: -5, state: playerReputation += 0.1)

Nghiêm mặt: “Buông bạn tao ra coi” → S6
(tension: +10, state: aggressorFocusOnPlayer = true; victimTrust += 0.1)

Giả vờ hỏi chuyện khác: “Ô, Hùng ơi, giáo viên chủ nhiệm đang tìm mày kìa” → S7
(tension: -5, state: hungReputation -= 0.1)

S3 – Đang quay kín
Mô tả: Điện thoại đang quay, bạn có thể vừa ghi hình vừa quyết định làm gì tiếp.

Lựa chọn:

Tiếp tục chỉ quay, chưa xen vào → S8
(tension: +5, state: rumorLevel += 0.1)

Gửi nhanh video cho Linh với caption “Ra đây giúp” → S4
(tension: 0, state: bystanderSupport = max(1, bystanderSupport))

Bỏ điện thoại vào túi, bước lại gần can thiệp → S5
(tension: -5, state: victimTrust += 0.1)

S4 – Linh xuất hiện
Mô tả: Linh chạy tới, thở hổn hển: “Có chuyện gì?”. Cả hai cùng nhìn về phía Hùng và Nam.

Lựa chọn:

Bàn nhanh với Linh: “Tí nữa mày kéo Nam, tao nói chuyện với Hùng” → S9
(tension: +0, state: bystanderSupport = 2)

Bảo Linh đi gọi cô, bạn ở lại quan sát → S10
(tension: 0, state: calledHelp = true; teacherOnTheWay = true; teacherTrust += 0.1)

Bảo Linh đứng quay video, bạn đứng ngoài coi → S11
(tension: +5, state: hasRecording = true; rumorLevel += 0.2; playerReputation -= 0.1)

S5 – Can thiệp bằng giọng nhẹ
Mô tả: Bạn bước tới gần, cố giữ tông giọng bình thường: “Ủa có chuyện gì mà gắt vậy, bình tĩnh tí đi?”.

Lựa chọn:

Hỏi Hùng: “Có gì nói từ từ, mày bực cái gì?” → S12
(tension: -10, state: hungReputation += 0.1)

Quay sang Nam: “Mày có ổn không? Muốn tao ở lại không?” → S13
(tension: -5, state: victimTrust += 0.3)

Lỡ miệng nói: “Đánh nhau trong trường là lên sổ đầu bài đó” → S14
(tension: +10, state: hungReputation -= 0.2; aggressorFocusOnPlayer = true)

S6 – Đối đầu thẳng
Mô tả: Bạn đứng chắn một phần giữa Hùng và Nam: “Buông bạn tao ra đi”. Hùng nhìn bạn trừng trừng.

Lựa chọn:

Giữ eye contact, nói chậm: “Tao không muốn gây chuyện, nhưng mày làm hơi quá rồi đó” → S15
(tension: +5, state: playerReputation += 0.2)

Hơi đẩy tay Hùng ra khỏi áo Nam → S17
(tension: +20, state: aggressorFocusOnPlayer = true; futureThreat = true)

Rút lui nửa bước, chuyển sang giọng nhẹ nhàng hơn → S12
(tension: -5, state: không đổi)

S7 – Đánh lạc hướng bằng… nói xạo
Mô tả: Bạn bịa chuyện giáo viên tìm Hùng. Hùng hơi khựng lại, liếc quanh.

Lựa chọn:

Thêm: “Hình như chuyện điếu thuốc trong nhà vệ sinh hôm qua” → S14
(tension: +10, state: hungReputation -= 0.3)

Nhân lúc Hùng phân tâm, ra hiệu cho Nam lùi lại → S16
(tension: -10, state: victimSafe = true; victimTrust += 0.2)

Khi Hùng hỏi: “Thật không?”, bạn thú nhận “Không… tao chỉ muốn mày dừng lại” → S12
(tension: -5, state: teacherTrust += 0.1 nếu giáo viên xuất hiện sau)

S8 – Chỉ đứng quay (route “shadow witness”)
Mô tả: Bạn tiếp tục quay. Hình ảnh trong khung hình càng lúc càng căng.

Lựa chọn:

Chỉ quay, sau đó định gửi cho Nam sau này → E1
(Ending “im lặng làm nhân chứng”: tension có thể giảm hoặc tăng nhẹ tùy roll, nhưng không can thiệp)

Dừng quay, gọi điện trực tiếp cho cô chủ nhiệm → S10
(tension: 0, state: calledHelp = true; teacherOnTheWay = true; hasRecording = true)

Vừa quay vừa hét: “Này đủ rồi đó Hùng!” → S6
(tension: +10, state: recordingDiscovered = true; aggressorFocusOnPlayer = true)

S9 – Phối hợp với Linh
Mô tả: Hai bạn chia việc, Linh chuẩn bị kéo Nam, bạn đối thoại với Hùng.

Lựa chọn:

Ra hiệu cho Linh kéo Nam khi bạn bắt đầu nói → S16
(tension: -5, state: victimSafe = true; victimTrust += 0.3)

Đổi ý, bảo Linh đứng yên để tránh rối thêm, tự bạn nói riêng với Hùng → S12
(tension: -5, state: bystanderSupport vẫn = 2)

S10 – Gọi người lớn
Mô tả: Bạn gọi cô/giám thị, nói vắn tắt: “Hành lang A đang có vụ căng, cô ra giúp với.”

Branches (logic):

Nếu gọi sớm, tension < SOFT_THRESHOLD trong vài bước → S18

Nếu gọi muộn, tension đã leo cao hoặc bạn từng khiêu khích → có thể nổ S17 trước khi cô tới → E4

(Không có lựa chọn trực tiếp, đây là node logic.)

S11 – Linh quay, bạn đứng ngoài
Mô tả: Linh cầm máy quay, bạn khoanh tay đứng xem, cảnh tượng bắt đầu thu hút vài ánh nhìn xa xa.

Lựa chọn:

Thì thầm với Linh: “Đừng đăng lên mạng, chỉ để làm bằng chứng thôi” → S12
(tension: -5, state: rumorLevel += 0.1)

Hào hứng: “Để lát gửi cho nhóm chat coi, tụi nó sốc luôn” → E6
(Ending drama mạng xã hội: socialMediaPosted = true; playerReputation += 0.1 trong friend group nhưng -0.5 với giáo viên; hungReputation -= 0.6; rumorLevel = 1.0)

S12 – Nghe Hùng kể
Mô tả: Bạn cho Hùng cơ hội nói: “Rồi, kể tao nghe chuyện gì trước đã”. Hùng bắt đầu xả: Nam bị nghi là đã kể chuyện Hùng hút thuốc.

Lựa chọn:

Thừa nhận: “Nếu là tao, tao cũng tức, nhưng đánh người thì…” → S19
(tension: -15, state: hungReputation += 0.2)

Đổi câu chuyện sang hướng “tụi mình cùng đang căng vì thi cử, đừng trút lên nhau” → S15
(tension: -5, state: playerReputation += 0.1)

Buột miệng: “Thì mày hút thì chịu, mắc gì đổ cho người khác?” → S14
(tension: +10, state: hungReputation -= 0.2; aggressorFocusOnPlayer = true)

S13 – Đứng về phía Nam
Mô tả: Bạn hỏi Nam ngay trước mặt Hùng: “Mày muốn tao ở lại không?”. Nam nhỏ giọng: “Đừng đi…”.

Lựa chọn:

Nói rõ: “Ok, tao ở đây cho tới khi mày thấy an toàn đã” → S15
(tension: -5, state: victimTrust += 0.4)

Nháy mắt với Nam, ra hiệu lát gặp ở cầu thang sau để nói riêng → E2
(Ending “hẹn gặp sau để hỗ trợ”: victimSafe tạm thời chưa chắc, nhưng victimTrust tăng; hậu quả tiếp theo để player tự nghĩ)

S14 – Lỡ lời dọa kỷ luật
Mô tả: Bạn nhắc tới chuyện sổ đầu bài, kỷ luật. Hùng nhíu mày, giọng gắt hơn.

Lựa chọn:

Nhận ra mình hơi lố, nhanh chóng đổi lại: “Ý tao là… đừng để tụi mình dính rắc rối thêm” → S15
(tension: -5, state: playerReputation += 0.1)

Vẫn giữ thái độ “tao nói đúng luật mà”, không đổi giọng → S17
(tension: +10, state: futureThreat = true)

S15 – Giữ lập trường nhưng không công kích
Mô tả: Bạn cố giữ vững quan điểm “dừng lại” nhưng tránh chọc vào tự ái của Hùng.

Lựa chọn:

Đề nghị cả ba ra khỏi hành lang, tìm chỗ ít người nói chuyện → S20
(tension: -10, state: victimSafe = true)

Nói: “Nếu mày vẫn muốn nói tiếp, cứ nói, nhưng tay mày bỏ khỏi cổ áo bạn tao trước đã” → S21
(tension: -5, state: aggressorFocusOnPlayer = true)

S16 – Kéo được Nam ra
Mô tả: Nhờ đánh lạc hướng, Nam bước lùi về phía bạn, mặt vẫn còn tái.

OnEnter:

victimSafe = true;

victimTrust += 0.3;

Lựa chọn:

“Mày muốn đi khỏi đây luôn không? Tao đi với mày” → E3
(Ending rút lui cùng nạn nhân: playerReputation +0.2, hungReputation không đổi, futureThreat có thể vẫn = true)

“Đứng đây với tao, để tao nói chuyện với Hùng chút” → S15
(tension: -5, state: không đổi)

S17 – Bạo lực nổ ra
Mô tả: Không khí vỡ vụn. Có xô đẩy, có người ngã, có thể là Nam, có thể là bạn, tùy tension & ai đang bị Hùng chú ý.

Ending: → E4 – Bạo lực xảy ra

Nếu aggressorFocusOnPlayer = true → player bị vạ lây.

Nếu không → Nam là người chịu nhiều hơn.

futureThreat có thể set true nếu player từng đối đầu mạnh.

S18 – Người lớn xuất hiện kịp
Mô tả: Cô/giám thị đi tới, nhìn thấy cảnh Hùng đang dí sát Nam, bạn đang ở gần.

Ending: → E5 – Người lớn can thiệp kịp thời

Kết quả chi tiết có thể tùy vào teacherTrust, hasRecording, hungReputation.

S19 – Công nhận cảm xúc
Mô tả: Bạn nói: “Tao hiểu bị nghi là ‘chơi dơ’ khó chịu lắm. Nhưng nếu mày đánh người, mọi người chỉ thấy mày là thằng bạo lực thôi.”

Lựa chọn:

Đề nghị tạm dừng hôm nay, hẹn Hùng và Nam nói chuyện với giáo viên cố vấn sau → E7
(Ending “hòa giải dẫn hướng”: tension giảm, hungReputation nhích lên, teacherTrust +0.3)

Đề nghị để Nam đi trước, bạn ở lại nói riêng thêm với Hùng → S20
(tension: -10, state: victimSafe = true)

S20 – Nói chuyện ở chỗ khác
Mô tả: Cả nhóm di chuyển sang chỗ ít người, không còn cảnh “dí vào tường”. Căng thẳng giảm, nhưng cảm xúc vẫn còn.

Lựa chọn:

Khuyến khích Hùng nói hết và Nam nghe, bạn đóng vai người “giữ luật chơi” → E8
(Ending “vòng tròn nói chuyện”: không ai bị đánh, nhưng kết quả chi tiết tùy script sau)

Đề nghị dừng tại đây, ai về lớp nấy, bạn hẹn Nam sau → E2
(victimTrust tăng, hungReputation không đổi nhiều)

S21 – Đặt điều kiện rõ ràng
Mô tả: Bạn nói: “Muốn nói gì thì nói, nhưng tay bỏ khỏi người ta trước đã”. Hùng gằn giọng, nhưng từ từ buông tay.

Lựa chọn:

Sau khi Hùng buông, bạn nhắc Nam: “Không ổn thì nói hiệu cho tao liền, tao gọi cô ngay” → E2
(Ending “ở lại làm chỗ dựa”, victimTrust cao)

Sau khi Hùng buông, bạn rút nhẹ Nam ra sau, coi như kết thúc → E3
(Ending “rút lui an toàn”)

S22 – Hỗ trợ Nam sau sự việc (Aftercare)
(Node này có thể giữ lại từ bản cũ nếu bạn muốn thêm một ending thuần chăm sóc tâm lý; ở bản V2 mình đã gộp ý đó vào E2, E3, E7, E8.)

DANH SÁCH ENDING V2 (đa dạng hơn)
E0 – Bỏ đi
Bạn chọn không xen vào. Nam nhớ rất rõ việc bạn đã nhìn thấy mà vẫn quay lưng.

E1 – Im lặng làm nhân chứng
Bạn quay lại toàn bộ nhưng không can thiệp. Clip có thể giúp về sau, nhưng hôm đó Nam vẫn phải tự chịu trận.

E2 – Hẹn gặp sau để hỗ trợ
Bạn không giải quyết trọn vẹn ngay lúc đó, nhưng chủ động làm chỗ dựa cho Nam sau này (aftercare route).

E3 – Rút lui cùng nạn nhân
Bạn đưa Nam ra khỏi chỗ nguy hiểm, nhưng câu chuyện với Hùng chưa được giải quyết, có nguy cơ bùng lại sau.

E4 – Bạo lực xảy ra
Dù bạn chọn kiểu gì, tension vượt ngưỡng và đã có người bị đánh. Có thể là Nam, cũng có thể là bạn nếu từng chọc vào tự ái của Hùng.

E5 – Người lớn can thiệp kịp thời
Bạn (hoặc Linh) gọi người lớn đủ sớm, không ai bị đánh, mọi thứ chuyển sang “họp xử lý” ở cấp nhà trường.

E6 – Drama mạng xã hội
Clip bị đăng lên nhóm chat / mạng, cả trường bàn tán. Hùng bị bêu xấu, Nam bị “coi như drama”, còn bạn mang tiếng “thích quay drama”.

E7 – Hòa giải dẫn hướng
Bạn kéo câu chuyện về hướng “hẹn gặp giáo viên tư vấn”, Hùng bớt nóng và chịu thử nói chuyện đàng hoàng.

E8 – Vòng tròn nói chuyện
Cả ba chuyển sang ngồi nói chuyện ở chỗ yên tĩnh, không đánh nhau, nhưng người chơi vẫn phải điều phối cảm xúc hai bên.

markdown
Sao chép mã

---

Điểm khác biệt chính so với bản cũ:

- Không còn kiểu “lựa chọn = label mechanic” (Record/Call Help) mà là **câu nói, thái độ, quyết định rất đời thường**.
- Cùng một hành vi “quay clip” nhưng có thể dẫn tới:
  - E1 (im lặng),
  - E6 (drama mạng xã hội),
  - E5/E7 (bằng chứng giúp xử lý đúng).
- Không phải ending nào cũng trắng–đen; có **khoảng xám** (E2, E3, E7, E8).
- Nhiều chỗ **người chơi không đoán chắc** là lựa chọn sẽ dẫn tới bạo lực hay hòa giải, tùy tension hiện tại + biến khác.

Nếu bạn muốn, bước sau mình có thể:

- Chuyển luôn bộ node này sang format JSON/ScriptableObject-friendly (id, description, choices…) để bạn feed vào hệ `DialougeSO` + `tensionImpact` + `StateEffect`.
::contentReference[oaicite:0]{index=0}