export enum ExerciseCategory {
  Cardio = "Cardio",
  Strength = "Strength",
  Flexibility = "Flexibility",
}

export enum MuscleGroup {
  Chest = "Chest",
  Back = "Back",
  Shoulders = "Shoulders",
  Biceps = "Biceps",
  Triceps = "Triceps",
  Forearms = "Forearms",
  Legs = "Legs",
  Glutes = "Glutes",
  Hamstrings = "Hamstrings",
  Quadriceps = "Quadriceps",
  Calves = "Calves",
  Core = "Core",
  FullBody = "FullBody",
}

export interface ExerciseResponse {
  id: number;
  name: string;
  description: string | null;
  category: ExerciseCategory;
  muscleGroup: MuscleGroup;
}

export interface ExerciseFilters {
  category?: ExerciseCategory;
  muscleGroup?: MuscleGroup;
  search?: string;
}
