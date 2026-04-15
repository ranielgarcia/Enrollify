export type TeacherSubject = {
  id: number;
  code: string;
  title: string;
};

export type TeacherCollege = {
  id: number;
  name: string;
};

export type TeacherDepartment = {
  id: number;
  name: string;
};

export type Teacher = {
  id: number;
  profilePicture?: string;
  firstName: string;
  lastName: string;
  middleName: string;
  email: string;
  phoneNumber: string;
  college: TeacherCollege;
  academicTitle: string;
  department: TeacherDepartment;
  qualification: string;
  specialization: string;
  officeLocation: string;
  officeHours: string;
  biography?: string;
  subjects: TeacherSubject[];
  createdAt: string;
};
