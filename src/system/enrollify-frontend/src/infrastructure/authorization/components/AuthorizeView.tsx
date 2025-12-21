import { Authorized, type AuthorizedProps } from "./Authorized";

export const AuthorizeView: React.FC<AuthorizedProps> = ({
  children,
  unauthorized,
  ...props
}) => {
  return (
    <Authorized {...props} unauthorized={unauthorized}>
      {children}
    </Authorized>
  );
};
