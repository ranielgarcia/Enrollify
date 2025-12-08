import { Authorized, type AuthorizedProps } from "./Authorized";

interface AuthorizeViewProps extends AuthorizedProps {
  authorized: React.ReactNode;
}

export const AuthorizeView: React.FC<AuthorizeViewProps> = ({
  authorized,
  unauthorized,
  ...props
}) => {
  return (
    <Authorized {...props} unauthorized={unauthorized}>
      {authorized}
    </Authorized>
  );
};
