import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/components/ui/alert-dialog";
import { Badge } from "@/components/ui/badge";
import { Loader2, CheckCircle2, XCircle } from "lucide-react";
import { cn } from "@/lib/utils";

interface BulkStatusTransitionDialogProps {
  isOpen: boolean;
  onOpenChange: (open: boolean) => void;
  action: "open" | "cancel";
  sectionCount: number;
  onConfirm: () => void;
  isPending: boolean;
}

export function BulkStatusTransitionDialog({
  isOpen,
  onOpenChange,
  action,
  sectionCount,
  onConfirm,
  isPending,
}: BulkStatusTransitionDialogProps) {
  const isOpen_ = action === "open";

  return (
    <AlertDialog open={isOpen} onOpenChange={onOpenChange}>
      <AlertDialogContent>
        <AlertDialogHeader>
          <AlertDialogTitle className="flex items-center gap-2">
            {isOpen_ ? (
              <>
                <CheckCircle2 className="size-5 text-emerald-500" />
                Open Sections for Enrollment
              </>
            ) : (
              <>
                <XCircle className="size-5 text-destructive" />
                Cancel Sections
              </>
            )}
          </AlertDialogTitle>
          <AlertDialogDescription asChild>
            <div className="space-y-3">
              <p>
                You are about to{" "}
                <span className="font-semibold text-foreground">
                  {isOpen_ ? "open" : "cancel"}
                </span>{" "}
                <Badge variant="secondary" className="text-sm tabular-nums">
                  {sectionCount} section
                  {sectionCount !== 1 ? "s" : ""}
                </Badge>{" "}
                {isOpen_ ? "for enrollment." : "."}
              </p>

              {isOpen_ && (
                <div className="rounded-md border bg-muted/50 p-3 text-sm space-y-1">
                  <p className="font-medium text-foreground">
                    Before opening:
                  </p>
                  <ul className="list-disc list-inside space-y-0.5 text-muted-foreground">
                    <li>
                      Each section is validated individually by the backend
                    </li>
                    <li>
                      Sections with Error-severity issues will not be opened
                    </li>
                    <li>
                      Successfully opened sections will be available for
                      student enrollment
                    </li>
                  </ul>
                </div>
              )}

              {!isOpen_ && (
                <div className="rounded-md border border-destructive/20 bg-destructive/5 p-3 text-sm space-y-1">
                  <p className="font-medium text-foreground">
                    Effect of cancelling:
                  </p>
                  <ul className="list-disc list-inside space-y-0.5 text-muted-foreground">
                    <li>Teacher and room assignments will be released</li>
                    <li>Sections will be removed from conflict detection</li>
                    <li>This action cannot be undone</li>
                  </ul>
                </div>
              )}
            </div>
          </AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter>
          <AlertDialogCancel disabled={isPending}>Go Back</AlertDialogCancel>
          <AlertDialogAction
            disabled={isPending}
            onClick={(e) => {
              e.preventDefault();
              onConfirm();
            }}
            className={cn(
              isOpen_
                ? "bg-emerald-600 text-white hover:bg-emerald-700"
                : "bg-destructive text-destructive-foreground hover:bg-destructive/90",
            )}
          >
            {isPending && <Loader2 className="mr-2 size-4 animate-spin" />}
            {isOpen_
              ? `Open ${sectionCount} Section${sectionCount !== 1 ? "s" : ""}`
              : `Cancel ${sectionCount} Section${sectionCount !== 1 ? "s" : ""}`}
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
}
