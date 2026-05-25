import Table from "@mui/material/Table";
import TableBody from "@mui/material/TableBody";
import TableCell from "@mui/material/TableCell";
import TableContainer from "@mui/material/TableContainer";
import TableHead from "@mui/material/TableHead";
import TableRow from "@mui/material/TableRow";
import Typography from "@mui/material/Typography";
import type { MuscleGroupVolumeResponse } from "@/types/report";

interface MuscleGroupTableProps {
  data: MuscleGroupVolumeResponse;
}

export function MuscleGroupTable({ data }: MuscleGroupTableProps) {
  const sorted = [...data.items].sort((a, b) => b.totalVolumeKg - a.totalVolumeKg);

  if (sorted.length === 0) {
    return (
      <Typography color="text.secondary" sx={{ py: 2 }}>
        No data for this period
      </Typography>
    );
  }

  return (
    <TableContainer>
      <Table size="small">
        <TableHead>
          <TableRow>
            <TableCell>Muscle group</TableCell>
            <TableCell align="right">Volume (kg)</TableCell>
            <TableCell align="right">Sets logged</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {sorted.map((row) => (
            <TableRow key={row.muscleGroup} hover>
              <TableCell>{row.muscleGroup}</TableCell>
              <TableCell align="right">{row.totalVolumeKg.toLocaleString()}</TableCell>
              <TableCell align="right">{row.sessionExerciseLineCount}</TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </TableContainer>
  );
}
