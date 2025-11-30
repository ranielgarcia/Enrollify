import { createFileRoute } from "@tanstack/react-router";
import SomeComponent from "./somecomponent";

export const Route = createFileRoute("/contactus/")({
  component: Index,
});

function Index() {
  return (
    <>
      <h2>Contact us (Directory)</h2>
      <SomeComponent />
    </>
  );
}
