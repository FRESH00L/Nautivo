export class ApiError extends Error {
  status: number;
  constructor(status: number, message: string) {
    super(message);
    this.status = status;
  }
}

let organizationId: string | null = null;
export const setApiOrganization = (id: string | null) => {
  organizationId = id;
};

export async function api<T = unknown>(path: string, options: { method?: string; body?: unknown } = {}): Promise<T> {
  const headers: Record<string, string> = { "X-Requested-With": "nautivo" };
  if (organizationId) headers["X-Organization-Id"] = organizationId;
  if (options.body !== undefined) headers["Content-Type"] = "application/json";

  let response: Response;
  try {
    response = await fetch(`/api${path}`, {
      method: options.method ?? "GET",
      headers,
      credentials: "same-origin",
      body: options.body === undefined ? undefined : JSON.stringify(options.body),
    });
  } catch {
    throw new ApiError(0, "Brak połączenia z serwerem. Sprawdź, czy API działa.");
  }

  if (!response.ok) {
    if (response.status === 429) throw new ApiError(429, "Zbyt wiele prób. Odczekaj minutę i spróbuj ponownie.");
    const problem = await response.json().catch(() => null);
    const validation = problem?.errors ? (Object.values(problem.errors) as string[][])[0]?.[0] : undefined;
    throw new ApiError(response.status, validation ?? problem?.detail ?? "Nie udało się wykonać operacji. Spróbuj ponownie.");
  }
  if (response.status === 204) return undefined as T;
  return (await response.json()) as T;
}

export interface OrganizationInfo { id: string; name: string; modules: string[] }
export interface Me { id: string; email: string; fullName: string; isAdmin: boolean; organizations: OrganizationInfo[] }

export type VesselStatus = "InService" | "OutOfService";
export interface VesselBlock { id: string; startDate: string; endDate: string; reason: string }
export type VesselKind = "Yacht" | "Sailboat" | "Motorboat" | "Jetski" | "Kayak" | "Other";
export interface ChecklistItem { name: string; isOk: boolean; note: string | null }
export interface Vessel {
  id: string; name: string; model: string; lengthMeters: number; berths: number; registrationNumber: string | null;
  inspectionDue: string | null; status: VesselStatus; notes: string | null; upcomingCruises: number; blocks: VesselBlock[];
  kind: VesselKind; charterDayRate: number | null; upcomingCharters: number; checklist: ChecklistItem[];
}

export interface Charter {
  id: string; vesselId: string; vesselName: string; vesselKind: VesselKind; participantId: string; clientName: string; clientEmail: string | null; clientPhone: string | null;
  startDate: string; endDate: string; days: number; dayRate: number; total: number; notes: string | null;
}
export interface CharterClient { id: string; fullName: string; email: string | null; phone: string | null }
export interface CharterMonth { month: number; bookings: number; days: number; revenue: number }
export interface CharterUnitSummary {
  vesselId: string; name: string; kind: VesselKind; dayRate: number | null; status: VesselStatus;
  bookings: number; days: number; listValue: number; revenue: number; discounts: number; utilizationPercent: number; checklistMissing: number; checklistTotal: number;
}
export interface CharterSummary {
  year: number; units: number; unitsAvailable: number; clients: number; clientsThisYear: number;
  revenueYear: number; revenueThisMonth: number; bookingsYear: number; daysYear: number; upcoming: number; checklistMissing: number;
  months: CharterMonth[]; perUnit: CharterUnitSummary[];
}

export type CruiseStatus = "Planned" | "Cancelled";
export interface RoutePoint { name: string; lat: number | null; lng: number | null }
export interface CruiseItem {
  id: string; name: string; departurePort: string; arrivalPort: string; startDate: string; endDate: string;
  vesselId: string | null; yachtName: string | null; yachtModel: string | null; yachtLengthMeters: number | null;
  price: number; deposit: number; balanceDueDate: string; capacity: number; status: CruiseStatus; isConfirmed: boolean;
  confirmed: number; collected: number; expected: number; overdue: number; costs: number;
}
export type PaymentState = "Paid" | "Partial" | "None" | "Overdue" | "Overpaid";
export type EnrollmentStatus = "Confirmed" | "Waitlist" | "Cancelled";
export interface Payment { id: string; amount: number; paidOn: string; method: string; note: string | null }
export interface Enrollment {
  id: string; participantId: string; fullName: string; email: string | null; phone: string | null; experience: string | null;
  status: EnrollmentStatus; total: number; paid: number; remaining: number; state: PaymentState;
  createdAt: string; payments: Payment[];
}
export interface CruiseCost { id: string; category: string; description: string | null; amount: number }
export interface CruiseDetail { cruise: CruiseItem; description: string | null; routePoints: RoutePoint[]; enrollments: Enrollment[]; costs: CruiseCost[] }

export type ContactStage = "Lead" | "Customer" | "Regular" | "Lapsed";
export interface ParticipantItem {
  id: string; fullName: string; email: string | null; phone: string | null; experience: string | null; source: string | null; tags: string[];
  marketingConsent: boolean; stage: ContactStage; cruises: number; lastCruiseEnd: string | null; nextCruiseStart: string | null;
  paid: number; remaining: number; openTasks: number; nextTaskDue: string | null; createdAt: string; anonymized: boolean;
  qualifications: CourseType[];
}
/** Rejs albo szkolenie w historii klienta: ustawione jest cruiseId albo courseId. */
export interface ParticipantHistory {
  enrollmentId: string; cruiseId: string | null; courseId: string | null; cruiseName: string; startDate: string; endDate: string; status: EnrollmentStatus; total: number; paid: number;
  state: PaymentState; enrolledAt: string; payments: { amount: number; paidOn: string; method: string }[];
}
export type ActivityType = "Note" | "Call" | "Email" | "Meeting";
export interface Activity { id: string; type: ActivityType; text: string; occurredAt: string; author: string | null; isSystem: boolean }
export interface ContactTask { id: string; participantId: string; participantName: string; title: string; dueDate: string; doneAt: string | null }
export interface ParticipantDetail {
  id: string; fullName: string; email: string | null; phone: string | null; experience: string | null; emergencyContact: string | null; notes: string | null;
  source: string | null; tags: string[]; marketingConsent: boolean; marketingConsentAt: string | null; createdAt: string; anonymizedAt: string | null;
  manualQualifications: CourseType[]; earnedQualifications: EarnedQualification[];
  stage: ContactStage; history: ParticipantHistory[]; activities: Activity[]; tasks: ContactTask[];
}

export interface Dashboard {
  cruisesTotal: number; cruisesUpcoming: number; cruisesFinished: number; cruisesCancelled: number;
  earnedTotal: number; earnedThisMonth: number; earnedThisYear: number; toCollect: number; overdueAmount: number; overdueCount: number;
  participantsTotal: number; seatsTaken: number; seatsTotal: number; waitlistCount: number; vesselsTotal: number; vesselsAvailable: number;
  last30Days: { reservations: number; reservationsPrevious: number; income: number; incomePrevious: number; newClients: number; newClientsPrevious: number; cruisesStarting: number };
  yourCruises: { id: string; name: string; startDate: string; location: string; reservations: number }[];
  nextCruises: { id: string; name: string; startDate: string; endDate: string; vesselName: string | null; confirmed: number; capacity: number; remaining: number; location: string; isConfirmed: boolean }[];
  overdue: { enrollmentId: string; cruiseId: string | null; courseId: string | null; fullName: string; cruiseName: string; dueDate: string; remaining: number }[];
  vesselAlerts: { id: string; name: string; inspectionDue: string | null; status: VesselStatus; notes: string | null }[];
}

export interface TrendPoint { date: string; end: string; reservations: number; income: number }
export interface TrendResponse { days: number; bucket: string; points: TrendPoint[] }
export interface OrganizationSettings { id: string; name: string; contactEmail: string | null; phone: string | null; city: string | null; taxId: string | null; website: string | null; canEdit: boolean }

export interface CalendarItem { type: "cruise" | "block" | "charter" | "course"; id: string; title: string; startDate: string; endDate: string; confirmed: number | null; capacity: number | null }
export interface CalendarData { from: string; to: string; vessels: { id: string; name: string; model: string; status: VesselStatus; items: CalendarItem[] }[]; withoutVessel: CalendarItem[]; courses: CalendarItem[] }

export type CourseType = "Zj" | "Jsm" | "Stcw" | "Src" | "Smw" | "Other";
export type ExamResult = "Pending" | "Passed" | "Failed" | "Absent";
export type SessionKind = "Theory" | "Practice" | "Exam";
export interface EarnedQualification { type: CourseType; courseId: string; courseName: string; date: string; certificateNumber: string | null }
export interface CourseItem {
  id: string; type: CourseType; name: string; startDate: string; endDate: string; location: string; instructor: string | null; examDate: string | null;
  price: number; deposit: number; balanceDueDate: string; capacity: number; status: CruiseStatus;
  confirmed: number; collected: number; expected: number; overdue: number; sessions: number; sessionsDone: number; passed: number; costs: number;
  completedAt: string | null;
}
export interface CourseSession { id: string; date: string; startTime: string | null; endTime: string | null; kind: SessionKind; topic: string; notes: string | null; completedAt: string | null }
export interface Trainee { enrollment: Enrollment; examResult: ExamResult; certificateNumber: string | null; documentsDelivered: string[]; attendedSessions: string[] }
export interface CourseMaterial { id: string; title: string; url: string | null; note: string | null }
export interface CourseTest { id: string; title: string; date: string; maxScore: number; passScore: number; results: { enrollmentId: string; score: number }[] }
export interface CourseDetail {
  course: CourseItem; description: string | null; examInfo: string | null; requiredDocuments: string[];
  sessions: CourseSession[]; trainees: Trainee[]; materials: CourseMaterial[]; tests: CourseTest[]; costs: CruiseCost[];
}
